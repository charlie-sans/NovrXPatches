using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BaseX;
using QuantityX;
using FrooxEngine;
using CodeX;
using FrooxEngine.UIX;
namespace NovrX.UIX
{
    public class UIBuilder : UIBuilderBase
    {
        private Stack<Slot> roots = new Stack<Slot>();

        private Stack<UIStyle> _uiStyles = new Stack<UIStyle>();

        private bool rootIsLayout;

        private IAssetProvider<Sprite> _checkSprite;

        private IAssetProvider<Sprite> _circleSprite;

        public FrooxEngine.World World => Canvas.World;

        public Canvas Canvas { get; private set; }

        public Slot Root => roots.Peek();

        public Slot Current { get; private set; }

        public Slot LayoutTarget { get; private set; }

        public RectTransform CurrentRect => Current?.GetComponent<RectTransform>() ?? Root.GetComponent<RectTransform>();

        public UIStyle Style => _uiStyles.Peek();

        public RectTransform ForceNext { get; set; }

        public IAssetProvider<Sprite> CheckSprite
        {
            get
            {
                if (_checkSprite == null)
                {
                    _checkSprite = Root.World.GetSharedComponentOrCreate("BasicUI_Check", delegate (SpriteProvider sprite)
                    {
                        StaticTexture2D target = Root.World.RootSlot.AttachTexture(NeosAssets.Common.Icons.Check, true, true);
                        sprite.Texture.Target = target;
                    }, 0, false, true);
                }
                return _checkSprite;
            }
        }

        public IAssetProvider<Sprite> CircleSprite
        {
            get
            {
                if (_circleSprite == null)
                {
                    _circleSprite = Root.World.GetSharedComponentOrCreate("BasicUI_Circle", delegate (SpriteProvider sprite)
                    {
                        StaticTexture2D target = Root.World.RootSlot.AttachTexture(NeosAssets.Common.Particles.Disc, true, true);
                        sprite.Texture.Target = target;
                    }, 0, false, true);
                }
                return _circleSprite;
            }
        }

        public static void SetupButtonColor(Button button, OutlinedArc arc)
        {
            InteractionElement.ColorDriver colorDriver = button.ColorDrivers.Add();
            InteractionElement.ColorDriver outlineDriver = button.ColorDrivers.Add();
            colorDriver.ColorDrive.Target = arc.FillColor;
            outlineDriver.ColorDrive.Target = arc.OutlineColor;
            color c = new color(0.9f);
            colorDriver.SetColors( c);
            c = new color(0.1f);
            outlineDriver.SetColors( c);
        }

        public ArcData Arc([In] ref LocaleString label, bool setupButton = true)
        {
            Next("Arc");
            ArcData arcData = default(ArcData);
            arcData.arc = Current.AttachComponent<OutlinedArc>();
            arcData.arcLayout = Current.AttachComponent<ArcSegmentLayout>();
            if (setupButton)
            {
                arcData.button = Current.AttachComponent<Button>();
                SetupButtonColor(arcData.button, arcData.arc);
            }
            Nest();
            arcData.image = Image();
            arcData.arcLayout.Nested.Target = arcData.image.RectTransform;
            if (( label) != (LocaleString)null)
            {
                arcData.text = Text(ref label);
                arcData.arcLayout.Label.Target = arcData.text;
            }
            NestOut();
            return arcData;
        }

        public void PushStyle()
        {
            _uiStyles.Push(Style.Clone());
        }

        public void PopStyle()
        {
            _uiStyles.Pop();
        }

        public UIBuilder(RectTransform rect)
            : this(rect.Slot)
        {
        }

        public UIBuilder(Canvas canvas)
            : this(canvas.Slot)
        {
        }

        public UIBuilder(Slot root, Slot forceNext = null)
        {
            ForceNext = forceNext?.GetComponentOrAttach<RectTransform>();
            Canvas = root.GetComponent<Canvas>();
            if (Canvas == null)
            {
                root.GetComponentOrAttach<RectTransform>();
                Canvas = root.GetComponentInParents<Canvas>();
            }
            _uiStyles.Push(new UIStyle());
            roots.Push(root);
            Update();
        }

        public UIBuilder(Slot root, float canvasWidth, float canvasHeight, float canvasScale)
        {
            _uiStyles.Push(new UIStyle());
            Canvas = root.AttachComponent<Canvas>();
            Canvas.Size.Value = new float2(canvasWidth, canvasHeight);
            float3 v = float3.One;
            root.LocalScale = ( v) * canvasScale;
            roots.Push(root);
        }

        public void Nest()
        {
            LayoutTarget = null;
            if (Current == null)
            {
                throw new Exception("No Current element to nest into!");
            }
            if (Current != Root)
            {
                roots.Push(Current);
            }
            Current = null;
            Update();
        }

        public void NestInto(Slot slot)
        {
            NestInto(slot.GetComponentOrAttach<RectTransform>());
        }

        public void NestInto(RectTransform root)
        {
            LayoutTarget = null;
            roots.Push(root.Slot);
            Current = null;
            Update();
        }

        public void NestOut()
        {
            LayoutTarget = null;
            if (roots.Count > 1)
            {
                Current = roots.Pop();
                Update();
                return;
            }
            throw new Exception("No more roots to pop, at the top of the canvas!");
        }

        public Slot Next(string name)
        {
            LayoutTarget = null;
            if (ForceNext != null)
            {
                Current = ForceNext.Slot;
                ForceNext = null;
            }
            else
            {
                Current = Root.AddSlot(name);
                Current.GetComponentOrAttach<RectTransform>();
            }
            if (rootIsLayout)
            {
                SetupCurrentAsLayoutElement();
            }
            return Current;
        }

        private void SetupCurrentAsLayoutElement()
        {
            LayoutElement layoutElement = Current.AttachComponent<LayoutElement>();
            layoutElement.MinWidth.Value = Style.MinWidth;
            layoutElement.MinHeight.Value = Style.MinHeight;
            layoutElement.PreferredWidth.Value = Style.PreferredWidth;
            layoutElement.PreferredHeight.Value = Style.PreferredHeight;
            layoutElement.FlexibleWidth.Value = Style.FlexibleWidth;
            layoutElement.FlexibleHeight.Value = Style.FlexibleHeight;
            layoutElement.UseZeroMetrics.Value = Style.UseZeroMetrics;
        }

        private void NextForLayout(string name)
        {
            if (LayoutTarget != null)
            {
                Current = LayoutTarget;
            }
            else
            {
                Next(name);
            }
        }

        private void Update()
        {
            rootIsLayout = Root.GetComponent<HorizontalLayout>() != null || Root.GetComponent<VerticalLayout>() != null || Root.GetComponent<GridLayout>() != null;
        }

        public Text Text([In] ref LocaleString text, bool bestFit = true, Alignment? alignment = null, bool parseRTF = true, string nullContent = null)
        {
            Next("Text");
            Text text2 = Current.AttachComponent<Text>();
            text2.LocaleContent = text;
            text2.NullContent.Value = nullContent;
            text2.ParseRichText.Value = parseRTF;
            text2.Color.Value = Style.TextColor;
            text2.AutoSize = bestFit;
            text2.AutoSizeMin.Value = Style.TextAutoSizeMin;
            text2.AutoSizeMax.Value = Style.TextAutoSizeMax;
            text2.LineHeight.Value = Style.TextLineHeight;
            text2.Align = alignment ?? Style.TextAlignment;
            return text2;
        }

        public Text Text([In] ref LocaleString text, int size, bool bestFit = true, Alignment? alignment = null, bool parseRTF = true)
        {
            Text text2 = Text(ref text, bestFit, alignment, parseRTF);
            text2.Size.Value = size;
            return text2;
        }

        public Slot Empty(string name = "Slot")
        {
            Next(name);
            return Current;
        }

        public RectTransform Panel()
        {
            Next("Panel");
            Nest();
            return Root.GetComponent<RectTransform>();
        }

        public RectTransform Spacer(float size)
        {
            float _minWidth = Style.MinWidth;
            float _preferredWidth = Style.PreferredWidth;
            float _flexibleWidth = Style.FlexibleWidth;
            float _minHeight = Style.MinHeight;
            float _preferredHeight = Style.PreferredHeight;
            float _flexibleHeight = Style.FlexibleHeight;
            Style.Width = size;
            Style.Height = size;
            Style.FlexibleHeight = -1f;
            Style.FlexibleWidth = -1f;
            Empty("Spacer");
            Style.MinWidth = _minWidth;
            Style.PreferredWidth = _preferredWidth;
            Style.FlexibleWidth = _flexibleWidth;
            Style.MinHeight = _minHeight;
            Style.PreferredHeight = _preferredHeight;
            Style.FlexibleHeight = _flexibleHeight;
            return CurrentRect;
        }

        public List<RectTransform> SplitHorizontally(params float[] proportions)
        {
            MathX.NormalizeSum(proportions);
            List<RectTransform> panels = new List<RectTransform>();
            float currentPos = 0f;
            foreach (float p in proportions)
            {
                RectTransform rect = Empty("Split").GetComponent<RectTransform>();
                rect.AnchorMin.Value = new float2(currentPos);
                rect.AnchorMax.Value = new float2(currentPos + p, 1f);
                currentPos += p;
                panels.Add(rect);
            }
            return panels;
        }

        public List<RectTransform> SplitVertically(params float[] proportions)
        {
            MathX.NormalizeSum(proportions);
            List<RectTransform> panels = new List<RectTransform>();
            float currentPos = 1f;
            foreach (float p in proportions)
            {
                RectTransform rect = Empty("Split").GetComponent<RectTransform>();
                rect.AnchorMin.Value = new float2(0f, currentPos - p);
                rect.AnchorMax.Value = new float2(1f, currentPos);
                currentPos -= p;
                panels.Add(rect);
            }
            return panels;
        }

        public void SplitVertically(float proportion, out RectTransform top, out RectTransform bottom, float gap = 0f)
        {
            proportion = 1f - proportion;
            float halfGap = gap * 0.5f;
            top = Empty("Top").GetComponent<RectTransform>();
            bottom = Empty("Bottom").GetComponent<RectTransform>();
            top.AnchorMin.Value = new float2(0f, proportion + halfGap);
            top.AnchorMax.Value = new float2(1f, 1f);
            bottom.AnchorMin.Value = new float2(0f, 0f);
            bottom.AnchorMax.Value = new float2(1f, proportion - halfGap);
        }

        public void SplitHorizontally(float proportion, out RectTransform left, out RectTransform right, float gap = 0f)
        {
            float halfGap = gap * 0.5f;
            left = Empty("Left").GetComponent<RectTransform>();
            right = Empty("Right").GetComponent<RectTransform>();
            left.AnchorMin.Value = new float2(0f, 0f);
            left.AnchorMax.Value = new float2(proportion - halfGap, 1f);
            right.AnchorMin.Value = new float2(proportion + halfGap);
            right.AnchorMax.Value = new float2(1f, 1f);
        }

        public void HorizontalHeader(float size, out RectTransform header, out RectTransform content)
        {
            header = Empty("Header").GetComponent<RectTransform>();
            content = Empty("Content").GetComponent<RectTransform>();
            header.OffsetMin.Value = new float2(0f, 0f - size);
            header.AnchorMin.Value = new float2(0f, 1f);
            header.AnchorMax.Value = new float2(1f, 1f);
            content.OffsetMax.Value = new float2(0f, 0f - size);
        }

        public void HorizontalFooter(float size, out RectTransform footer, out RectTransform content)
        {
            content = Empty("Content").GetComponent<RectTransform>();
            footer = Empty("Footer").GetComponent<RectTransform>();
            footer.OffsetMax.Value = new float2(0f, size);
            footer.AnchorMin.Value = new float2(0f, 0f);
            footer.AnchorMax.Value = new float2(1f);
            content.OffsetMin.Value = new float2(0f, size);
        }

        public void VerticalHeader(float size, out RectTransform header, out RectTransform content)
        {
            header = Empty("Header").GetComponent<RectTransform>();
            content = Empty("Content").GetComponent<RectTransform>();
            header.OffsetMax.Value = new float2(size);
            header.AnchorMin.Value = new float2(0f, 0f);
            header.AnchorMax.Value = new float2(0f, 1f);
            content.OffsetMin.Value = new float2(size);
        }

        public void VerticalFooter(float size, out RectTransform footer, out RectTransform content)
        {
            content = Empty("Content").GetComponent<RectTransform>();
            footer = Empty("Footer").GetComponent<RectTransform>();
            footer.OffsetMin.Value = new float2(0f - size);
            footer.AnchorMin.Value = new float2(1f);
            footer.AnchorMax.Value = new float2(1f, 1f);
            content.OffsetMax.Value = new float2(0f - size);
        }

        public Image Panel([In] ref color tint, bool zwrite = false)
        {
            Image result = Image(ref tint, zwrite);
            Nest();
            return result;
        }

        public Button Button()
        {
            LocaleString text = "";
            return Button(ref text);
        }

        public Button Button([In] ref LocaleString text)
        {
            return Button(ref text, ref Style.ButtonColor);
        }

        public Button Button([In] ref LocaleString text, ButtonEventHandler action)
        {
            Button button = Button(ref text);
            button.Pressed.Target = action;
            return button;
        }

        public Button Button(Uri icon, ButtonEventHandler action)
        {
            Button button = Button(icon);
            button.Pressed.Target = action;
            return button;
        }

        public Button Button(Uri icon, LocaleString text)
        {
            return Button(ref text, null, icon, ref Style.ButtonColor, ref Style.ButtonSpriteColor);
        }

        public Button Button(Uri icon, LocaleString text, [In] ref color tint, [In] ref color spriteTint)
        {
            return Button(ref text, null, icon, ref tint, ref spriteTint);
        }

        public Button Button(Uri icon, LocaleString text, ButtonEventHandler action)
        {
            Button button = Button(icon, text);
            button.Pressed.Target = action;
            return button;
        }

        public Button Button(Uri icon, LocaleString text, [In] ref color tint, [In] ref color spriteTint, ButtonEventHandler action)
        {
            Button button = Button(icon, text, ref tint, ref spriteTint);
            button.Pressed.Target = action;
            return button;
        }

        public Button Button([In] ref LocaleString text, [In] ref color tint, ButtonEventHandler action, float doublePressDelay = 0f)
        {
            return Button(ref text, ref tint).SetupAction(action, doublePressDelay);
        }

        public Button Button(Uri spriteUrl)
        {
            return Button(spriteUrl, ref Style.ButtonColor);
        }

        public Button Button(Uri spriteUrl, [In] ref color buttonTint)
        {
            return Button(spriteUrl, ref buttonTint, ref Style.ButtonSpriteColor);
        }

        public Button Button(Uri spriteUrl, [In] ref color buttonTint, [In] ref color spriteTint)
        {
            LocaleString text = null;
            return Button(ref text, null, spriteUrl, ref buttonTint, ref spriteTint);
        }

        public Button Button(IAssetProvider<Sprite> sprite, [In] ref color buttonTint, [In] ref color spriteTint)
        {
            LocaleString text = null;
            return Button(ref text, sprite, null, ref buttonTint, ref spriteTint);
        }

        public Button Button(Uri spriteUrl, [In] ref color buttonTint, [In] ref color spriteTint, ButtonEventHandler action, float doublePressDelay = 0f)
        {
            LocaleString text = null;
            return Button(ref text, null, spriteUrl, ref buttonTint, ref spriteTint).SetupAction(action, doublePressDelay);
        }

        public Button Button<T>(Uri spriteUrl, [In] ref color buttonTint, [In] ref color spriteTint, ButtonEventHandler<T> action, T argument, float doublePressDelay = 0f)
        {
            LocaleString text = null;
            Button button = Button(ref text, null, spriteUrl, ref buttonTint, ref spriteTint);
            button.SetupAction(action, argument, doublePressDelay);
            return button;
        }

        public Button Button([In] ref LocaleString text, [In] ref color tint)
        {
            color spriteTint = color.White;
            return Button(ref text, null, null, ref tint, ref spriteTint);
        }

        private Button Button([In] ref LocaleString text, IAssetProvider<Sprite> sprite, Uri spriteUrl, [In] ref color tint, [In] ref color spriteTint)
        {
            Next("Button");
            Current.AttachComponent<Image>().Tint.Value = tint;
            Button button = Current.AttachComponent<Button>();
            button.PassThroughHorizontalMovement.Value = Style.PassThroughHorizontalMovement;
            button.PassThroughVerticalMovement.Value = Style.PassThroughVerticalMovement;
            if (Style.DisabledColor.HasValue)
            {
                button.ColorDrivers[0].DisabledColor.Value = Style.DisabledColor.Value;
            }
            bool num = spriteUrl != null || sprite != null;
            RectTransform spriteRoot = null;
            RectTransform textRoot = null;
            Nest();
            if (num && ( text) != (LocaleString)null)
            {
                SplitHorizontally(0.3333f, out spriteRoot, out textRoot, 0.05f);
            }
            if (num)
            {
                if (spriteRoot != null)
                {
                    ForceNext = spriteRoot;
                }
                Image icon = Image(null, ref spriteTint);
                if (spriteUrl != null)
                {
                    icon.Sprite.Target = Current.AttachSprite(spriteUrl);
                }
                icon.RectTransform.AddFixedPadding(Style.ButtonIconPadding);
                if (Style.DisabledAlpha.HasValue)
                {
                    button.SetupTransparentOnDisabled(icon.Tint, Style.DisabledAlpha.Value);
                }
            }
            if (( text) != (LocaleString)null)
            {
                if (textRoot != null)
                {
                    ForceNext = textRoot;
                }
                Text buttonLabel = Text(ref text, true, Alignment.MiddleCenter);
                buttonLabel.RectTransform.AddFixedPadding(Style.ButtonTextPadding);
                if (Style.DisabledAlpha.HasValue)
                {
                    button.SetupTransparentOnDisabled(buttonLabel.Color, Style.DisabledAlpha.Value);
                }
            }
            NestOut();
            return button;
        }

        public Button Button<T>([In] ref LocaleString text, ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0f)
        {
            Button button = Button(ref text, ref Style.ButtonColor);
            button.SetupAction(callback, argument, doublePressDelay);
            return button;
        }

        public Button Button<T>([In] ref LocaleString text, [In] ref color tint, ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0f)
        {
            Button button = Button(ref text, ref tint);
            button.SetupAction(callback, argument, doublePressDelay);
            return button;
        }

        public Button ButtonRef<T>([In] ref LocaleString text, [In] ref color tint, ButtonEventHandler<T> callback, T argument, float doublePressDelay = 0f) where T : class, IWorldElement
        {
            Button button = Button(ref text, ref tint);
            ButtonRefRelay<T> buttonRefRelay = button.Slot.AttachComponent<ButtonRefRelay<T>>();
            buttonRefRelay.Argument.Target = argument;
            buttonRefRelay.ButtonPressed.Target = callback;
            buttonRefRelay.DoublePressDelay.Value = doublePressDelay;
            return button;
        }

        public ValueRadio<T> ValueRadio<T>(IField<T> valueField, T value)
        {
            ValueRadio<T> valueRadio = Radio<ValueRadio<T>>();
            valueRadio.OptionValue.Value = value;
            valueRadio.TargetValue.Target = valueField;
            return valueRadio;
        }

        public ReferenceRadio<T> ReferenceRadio<T>(SyncRef<T> refField, T target) where T : class, IWorldElement
        {
            ReferenceRadio<T> referenceRadio = Radio<ReferenceRadio<T>>();
            referenceRadio.OptionReference.Target = target;
            referenceRadio.TargetReference.Target = refField;
            return referenceRadio;
        }

        public ValueRadio<T> ValueRadio<T>([In] ref LocaleString label, IField<T> valueField, T value)
        {
            Text text;
            return ValueRadio(ref label, valueField, value, out text);
        }

        public ValueRadio<T> ValueRadio<T>([In] ref LocaleString label, IField<T> valueField, T value, out Text text)
        {
            float size = MathX.Max(Style.MinHeight, Style.PreferredHeight);
            Panel();
            VerticalFooter(size, out var footer, out var content);
            NestInto(content);
            text = Text(ref label, true, Alignment.MiddleLeft);
            NestOut();
            NestInto(footer);
            ValueRadio<T> result = ValueRadio(valueField, value);
            NestOut();
            NestOut();
            return result;
        }

        public ReferenceRadio<T> ReferenceRadio<T>(string label, SyncRef<T> refField, T target) where T : class, IWorldElement
        {
            float size = MathX.Max(Style.MinHeight, Style.PreferredHeight);
            Panel();
            VerticalFooter(size, out var footer, out var content);
            NestInto(content);
            LocaleString text = label;
            Text(ref text, true, Alignment.MiddleLeft);
            NestOut();
            NestInto(footer);
            ReferenceRadio<T> result = ReferenceRadio(refField, target);
            NestOut();
            NestOut();
            return result;
        }

        public R Radio<R>() where R : Radio, new()
        {
            PushStyle();
            Style.MinWidth = Style.MinHeight;
            Style.PreferredWidth = Style.PreferredWidth;
            Panel();
            PopStyle();
            float size = MathX.Max(Style.MinHeight, Style.PreferredHeight);
            Image(CircleSprite).RectTransform.Pivot.Value = new float2(0f, 0.5f);
            Current.AttachComponent<AspectRatioFitter>();
            Current.AttachComponent<Button>();
            R val = Current.AttachComponent<R>();
            Nest();
            IAssetProvider<Sprite> circleSprite = CircleSprite;
            color tint = color.Black;
            Image selectImage = Image(circleSprite, ref tint);
            SetPadding(size * 0.1f);
            NestOut();
            NestOut();
            val.CheckVisual.Target = selectImage.Slot.ActiveSelf_Field;
            return val;
        }

        public Checkbox Checkbox([In] ref LocaleString label, bool state = false, bool labelFirst = true, float padding = 4f)
        {
            float size = MathX.Max(Style.MinHeight, Style.PreferredHeight);
            Panel();
            RectTransform checkboxRoot;
            RectTransform labelRoot;
            if (labelFirst)
            {
                VerticalFooter(size, out checkboxRoot, out labelRoot);
                labelRoot.AddFixedPadding(0f, padding, 0f, 0f);
            }
            else
            {
                VerticalHeader(size, out checkboxRoot, out labelRoot);
                labelRoot.AddFixedPadding(0f, 0f, 0f, padding);
            }
            NestInto(labelRoot);
            Text(ref label, true, Alignment.MiddleLeft);
            NestOut();
            NestInto(checkboxRoot);
            Checkbox checkbox = Checkbox();
            checkbox.State.Value = state;
            NestOut();
            NestOut();
            return checkbox;
        }

        public Checkbox Checkbox(bool state = false)
        {
            PushStyle();
            Style.MinWidth = Style.MinHeight;
            Panel();
            PopStyle();
            color color = color.White;
            Image(ref color).RectTransform.Pivot.Value = new float2(0f, 0.5f);
            Current.AttachComponent<AspectRatioFitter>();
            Current.AttachComponent<Button>();
            Checkbox checkbox = Current.AttachComponent<Checkbox>();
            Nest();
            IAssetProvider<Sprite> checkSprite = CheckSprite;
            color = color.Black;
            Image checkImage = Image(checkSprite, ref color);
            NestOut();
            NestOut();
            checkbox.CheckVisual.Target = checkImage.Slot.ActiveSelf_Field;
            checkbox.State.Value = state;
            return checkbox;
        }

        public RectMesh<M> RectMesh<M>(IAssetProvider<Material> material = null) where M : RectMeshSource, new()
        {
            Next("RectMesh");
            RectMesh<M> mesh = Current.AttachComponent<RectMesh<M>>();
            if (material != null)
            {
                mesh.Materials.Add(material);
            }
            return mesh;
        }

        public RawGraphic RawGraphic(IAssetProvider<Material> material = null, IAssetProvider<MaterialPropertyBlock> propertyBlock = null)
        {
            Next("RawGraphic");
            RawGraphic rawGraphic = Current.AttachComponent<RawGraphic>();
            rawGraphic.Material.Target = material;
            rawGraphic.PropertyBlock.Target = propertyBlock;
            return rawGraphic;
        }

        public RawImage RawImage(IAssetProvider<ITexture2D> texture)
        {
            color tint = color.White;
            return RawImage(texture, ref tint);
        }

        public RawImage RawImage(IAssetProvider<ITexture2D> texture, [In] ref color tint)
        {
            Next("RawImage");
            RawImage rawImage = Current.AttachComponent<RawImage>();
            rawImage.Texture.Target = texture;
            return rawImage;
        }

        public TiledRawImage TiledRawImage(IAssetProvider<ITexture2D> texture, [In] ref color tint)
        {
            Next("TiledRawImage");
            TiledRawImage tiledRawImage = Current.AttachComponent<TiledRawImage>();
            tiledRawImage.Texture.Target = texture;
            return tiledRawImage;
        }

        public Image Image()
        {
            color color = color.White;
            return Image(ref color);
        }

        public Image Image([In] ref color color, bool zwrite = false)
        {
            Image image = Image(null, ref color);
            if (zwrite)
            {
                image.Material.Target = World.GetDefaultUI_ZWrite();
            }
            return image;
        }

        public Image Image(Uri url, int? maxSize = null)
        {
            return Image(url, color.White, maxSize);
        }

        public Image Image(Uri url, color tint, int? maxSize = null)
        {
            Image image = Image(ref tint);
            image.Sprite.Target = Current.AttachSprite(url, false, false, true, maxSize);
            return image;
        }

        public Image Image(IAssetProvider<ITexture2D> tex)
        {
            SpriteProvider sprite = Root.AttachComponent<SpriteProvider>();
            sprite.Texture.Target = tex;
            return Image(sprite);
        }

        public Image Image(IAssetProvider<Sprite> sprite, bool preserveAspect = true)
        {
            color tint = color.White;
            return Image(sprite, ref tint, preserveAspect);
        }

        public Image Image(IAssetProvider<Sprite> sprite, [In] ref color tint, bool preserveAspect = true)
        {
            Next("Image");
            Image image = Current.AttachComponent<Image>();
            image.Tint.Value = tint;
            image.Sprite.Target = sprite;
            image.PreserveAspect.Value = preserveAspect;
            return image;
        }

        public Mask Mask()
        {
            color color = color.White;
            return Mask(ref color);
        }

        public Mask Mask([In] ref color color, bool showMaskGraphic = false, bool zwrite = false)
        {
            Image(ref color, zwrite);
            Mask mask = Current.AttachComponent<Mask>();
            mask.ShowMaskGraphic.Value = showMaskGraphic;
            return mask;
        }

        public Mask SpriteMask(IAssetProvider<Sprite> sprite, bool showMaskGraphic = false)
        {
            Image(sprite);
            Mask mask = Current.AttachComponent<Mask>();
            mask.ShowMaskGraphic.Value = showMaskGraphic;
            return mask;
        }

        public TextField PasswordField()
        {
            TextField textField = TextField("", false, null, false);
            textField.Text.MaskPattern.Value = "*";
            return textField;
        }

        public TextField TextField(string defaultText = "", bool undo = false, string undoDescription = null, bool parseRTF = true)
        {
            Button();
            TextField textField = Current.AttachComponent<TextField>();
            Text text = Current.GetComponentInChildren<Text>();
            text.ParseRichText.Value = parseRTF;
            textField.Text = text;
            textField.Editor.Target.Undo.Value = undo;
            textField.Editor.Target.UndoDescription.Value = undoDescription;
            textField.TargetString = defaultText;
            return textField;
        }

        public T HorizontalElementWithLabel<T>([In] ref LocaleString label, float separation, Func<T> elementBuilder, float gap = 0.01f) where T : Component
        {
            Text labelText;
            return HorizontalElementWithLabel(ref label, separation, elementBuilder, out labelText, gap);
        }

        public T HorizontalElementWithLabel<T>([In] ref LocaleString label, float separation, Func<T> elementBuilder, out Text labelText, float gap = 0.01f) where T : Component
        {
            Panel();
            List<RectTransform> subsections = SplitHorizontally(separation, gap, 1f - (separation + gap));
            NestInto(subsections[0]);
            labelText = Text(ref label, true, Alignment.MiddleLeft);
            NestOut();
            NestInto(subsections[2]);
            T result = elementBuilder();
            NestOut();
            NestOut();
            return result;
        }

        public IntTextEditorParser IntegerField(int min = int.MinValue, int max = int.MaxValue, int increments = 1, bool parseContinuously = true)
        {
            IntTextEditorParser intTextEditorParser = TextField().Editor.Target.Slot.AttachComponent<IntTextEditorParser>();
            intTextEditorParser.ParseContinuously.Value = parseContinuously;
            intTextEditorParser.Min.Value = min;
            intTextEditorParser.Max.Value = max;
            intTextEditorParser.Increments.Value = increments;
            return intTextEditorParser;
        }

        public FloatTextEditorParser FloatField(float min = float.MinValue, float max = float.MaxValue, int decimalPlaces = 2, string format = null, bool parseContinuously = true)
        {
            FloatTextEditorParser floatTextEditorParser = TextField().Editor.Target.Slot.AttachComponent<FloatTextEditorParser>();
            floatTextEditorParser.ParseContinuously.Value = parseContinuously;
            floatTextEditorParser.Min.Value = min;
            floatTextEditorParser.Max.Value = max;
            floatTextEditorParser.DecimalPlaces.Value = decimalPlaces;
            floatTextEditorParser.StringFormat.Value = format;
            return floatTextEditorParser;
        }

        public Slider<float> Slider(float height, float value = 0f, float min = 0f, float max = 1f, bool integers = false)
        {
            return Slider<float>(height, value, min, max, integers);
        }

        public Slider<float> Slider(float height, out Image line, out Image handle)
        {
            return Slider(height, 0f, 0f, 1f, false, out line, out handle);
        }

        public Slider<T> Slider<T>(float height, T? value = null, T? min = null, T? max = null, bool integers = false) where T : struct
        {
            Image line;
            Image handle;
            return Slider(height, value.GetValueOrDefault(), min.GetValueOrDefault(), max ?? Coder<T>.Identity, integers, out line, out handle);
        }

        public Slider<T> Slider<T>(float height, out Image line, out Image handle)
        {
            return Slider(height, default(T), default(T), Coder<T>.Identity, false, out line, out handle);
        }

        public Slider<T> Slider<T>(float height, T value, T min, T max, bool integers, out Image line, out Image handle)
        {
            color color = color.Gray;
            Next("Slider");
            Slider<T> slider = Current.AttachComponent<Slider<T>>();
            Nest();
            Next("Background");
            Current.AttachComponent<Image>().Tint.Value = color.Clear;
            float halfHeight = height * 0.5f;
            RectTransform component = Current.GetComponent<RectTransform>();
            component.OffsetMin.Value = new float2(halfHeight);
            component.OffsetMax.Value = new float2(0f - halfHeight);
            Nest();
            line = Image(ref color);
            line.RectTransform.SetFixedVertical((0f - height) / 5f, height / 5f, 0.5f);
            NestOut();
            Next("HandleArea");
            RectTransform component2 = Current.GetComponent<RectTransform>();
            component2.OffsetMin.Value = new float2(halfHeight);
            component2.OffsetMax.Value = new float2(0f - halfHeight);
            Nest();
            handle = Image(CircleSprite);
            handle.InteractionTarget.Value = false;
            RectTransform rectTransform = handle.RectTransform;
            float2 v = float2.One;
            float2 position = ( v) * (0f - halfHeight);
            float2 v2 = float2.One;
            float2 size = ( v2) * height;
            rectTransform.SetFixedRect(new Rect( position,  size));
            handle.Slot.Name = "Handle";
            slider.HandleAnchorMinDrive.Target = handle.RectTransform.AnchorMin;
            slider.HandleAnchorMaxDrive.Target = handle.RectTransform.AnchorMax;
            slider.Min.Value = min;
            slider.Max.Value = max;
            slider.Value.Value = value;
            slider.Integers.Value = integers;
            slider.ColorDrivers.Add().ColorDrive.Target = handle.Tint;
            NestOut();
            NestOut();
            return slider;
        }

        public HorizontalLayout HorizontalLayout(float spacing = 0f, float padding = 0f, Alignment? childAlignment = null)
        {
            return HorizontalLayout(spacing, padding, padding, padding, padding, childAlignment);
        }

        public HorizontalLayout HorizontalLayout(float spacing, float paddingTop, float paddingRight, float paddingBottom, float paddingLeft, Alignment? childAlignment = null)
        {
            NextForLayout("Horizontal Layout");
            HorizontalLayout horizontalLayout = Current.AttachComponent<HorizontalLayout>();
            horizontalLayout.Spacing.Value = spacing;
            horizontalLayout.PaddingTop.Value = paddingTop;
            horizontalLayout.PaddingRight.Value = paddingRight;
            horizontalLayout.PaddingBottom.Value = paddingBottom;
            horizontalLayout.PaddingLeft.Value = paddingLeft;
            horizontalLayout.ChildAlignment = childAlignment ?? Style.ChildAlignment;
            horizontalLayout.ForceExpandHeight.Value = Style.ForceExpandHeight;
            horizontalLayout.ForceExpandWidth.Value = Style.ForceExpandWidth;
            Nest();
            return horizontalLayout;
        }

        public VerticalLayout VerticalLayout(float spacing = 0f, float padding = 0f, Alignment? childAlignment = null)
        {
            return VerticalLayout(spacing, padding, padding, padding, padding, childAlignment);
        }

        public VerticalLayout VerticalLayout(float spacing, float paddingTop, float paddingRight, float paddingBottom, float paddingLeft, Alignment? childAlignment = null)
        {
            NextForLayout("Vertical Layout");
            VerticalLayout verticalLayout = Current.AttachComponent<VerticalLayout>();
            verticalLayout.Spacing.Value = spacing;
            verticalLayout.PaddingTop.Value = paddingTop;
            verticalLayout.PaddingRight.Value = paddingRight;
            verticalLayout.PaddingBottom.Value = paddingBottom;
            verticalLayout.PaddingLeft.Value = paddingLeft;
            verticalLayout.ChildAlignment = childAlignment ?? Style.ChildAlignment;
            verticalLayout.ForceExpandHeight.Value = Style.ForceExpandHeight;
            verticalLayout.ForceExpandWidth.Value = Style.ForceExpandWidth;
            Nest();
            return verticalLayout;
        }

        public GridLayout GridLayout()
        {
            float2 cellvalue = float2.One;
            float2 cellSize = (cellvalue) * 64;
            float2 spacing = float2.Zero;
            return GridLayout(ref cellSize, ref spacing);
        }

        public GridLayout GridLayout([In] ref float2 cellSize)
        {
            float2 spacing = float2.Zero;
            return GridLayout(ref cellSize, ref spacing);
        }

        public GridLayout GridLayout([In] ref float2 cellSize, [In] ref float2 spacing, Alignment childAlignment = Alignment.MiddleCenter)
        {
            NextForLayout("Grid Layout");
            GridLayout gridLayout = Current.AttachComponent<GridLayout>();
            gridLayout.Spacing.Value = spacing;
            gridLayout.CellSize.Value = cellSize;
            gridLayout.ChildAlignment = childAlignment;
            Nest();
            return gridLayout;
        }

        public ScrollRect ScrollArea(Alignment? alignment = null)
        {
            Mask mask;
            Image graphic;
            return ScrollArea<Image>(alignment, out mask, out graphic);
        }

        public ScrollRect ScrollArea<G>(Alignment? alignment, out Mask mask, out G graphic) where G : Graphic, new()
        {
            Next("Scroll Area");
            Slot content;
            ScrollRect rect = ScrollRect.CreateScrollRect<G>(Current, out content, out mask, out graphic);
            if (alignment.HasValue)
            {
                rect.Alignment = alignment.Value;
            }
            Current = content;
            Nest();
            LayoutTarget = content;
            return rect;
        }

        public ContentSizeFitter FitContent()
        {
            return FitContent(SizeFit.PreferredSize);
        }

        public ContentSizeFitter FitContent(SizeFit fit)
        {
            return FitContent(fit, fit);
        }

        public ContentSizeFitter FitContent(SizeFit horizontal, SizeFit vertical)
        {
            Slot target = Current ?? Root;
            ContentSizeFitter obj = target.GetComponent<ContentSizeFitter>() ?? target.AttachComponent<ContentSizeFitter>();
            obj.HorizontalFit.Value = horizontal;
            obj.VerticalFit.Value = vertical;
            return obj;
        }

        public void SetFixedSize([In] ref float2 anchor, [In] ref float2 size)
        {
            CurrentRect.AnchorMin.Value = anchor;
            CurrentRect.AnchorMax.Value = anchor;
            CurrentRect.OffsetMin.Value = float2.Zero;
            CurrentRect.OffsetMax.Value = size;
        }

        public void SetFixedHeight(float height, float heightAnchor = 0f)
        {
            CurrentRect.AnchorMin.Value = new float2(0f, heightAnchor);
            CurrentRect.AnchorMax.Value = new float2(1f, heightAnchor);
            CurrentRect.OffsetMin.Value = float2.Zero;
            CurrentRect.OffsetMax.Value = new float2(0f, height);
        }

        public void SetFixedWeight(float width, float widthAnchor = 0f)
        {
            CurrentRect.AnchorMin.Value = new float2(widthAnchor);
            CurrentRect.AnchorMax.Value = new float2(widthAnchor, 1f);
            CurrentRect.OffsetMin.Value = float2.Zero;
            CurrentRect.OffsetMax.Value = new float2(width);
        }

        public void SetPadding(float padding)
        {
            SetPadding(padding, padding, padding, padding);
        }

        public void SetPadding(float top, float right, float bottom, float left)
        {
            CurrentRect.OffsetMin.Value = new float2(left, top);
            CurrentRect.OffsetMax.Value = new float2(0f - right, 0f - bottom);
        }
    }
}
