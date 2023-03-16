﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.SpritesUi
{
    /// <summary>
    /// Logique d'interaction pour DwellingUi.xaml
    /// </summary>
    public partial class StructureUi : BaseSpriteUi<Structure>
    {
        private const int IndexZ = 1;
        private const double FocusStroke = 2;
        private const double SpaceBetween = 2;

        private static double StrokeAndSpace => FocusStroke + SpaceBetween;
        private static double TotalStrokeSize => StrokeAndSpace * 2;

        private static IReadOnlyDictionary<(Type, bool), Brush> _brushes = new Dictionary<(Type, bool), Brush>
        {
            { (typeof(Market), false), GetImageFill(Brushes.Purple, "market") },
            { (typeof(Market), true), GetImageFill(Brushes.MediumPurple, "market") },
            { (typeof(Dwelling), false), GetImageFill(Brushes.Sienna, "dwelling") },
            { (typeof(Dwelling), true), GetImageFill(Brushes.Peru, "dwelling") }
        };

        private readonly Rectangle _surround;
        private readonly Rectangle _visual;

        public StructureUi(Structure structure, bool isBlueprint)
            : base(structure)
        {
            InitializeComponent();

            SetValue(Panel.ZIndexProperty, IndexZ);

            _visual = new Rectangle
            {
                Width = Sprite.Surface.Width,
                Height = Sprite.Surface.Height,
                Fill = _brushes[(Sprite.GetType(), false)],
                Opacity = isBlueprint ? 0.5 : 1
            };
            MainCanvas.Children.Add(_visual);

            _surround = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = FocusStroke,
                Width = Sprite.Surface.Width + TotalStrokeSize,
                Height = Sprite.Surface.Height + TotalStrokeSize,
                Fill = Brushes.Transparent
            };

            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            if (!isBlueprint)
            {
                MouseEnter += (a, b) => _visual.Fill = _brushes[(Sprite.GetType(), true)];
                MouseLeave += (a, b) => _visual.Fill = _brushes[(Sprite.GetType(), false)];
                MouseLeftButtonDown += (a, b) => Sprite.ToggleFocus();

                Sprite.PropertyChanged += (s, e) =>
                {
                    Action action = null;
                    if (e.PropertyName == nameof(Sprite.Focused))
                    {
                        action = () =>
                        {
                            SetControlDimensionsAndPosition();
                            if (Sprite.Focused)
                                MainCanvas.Children.Add(_surround);
                            else
                                MainCanvas.Children.Remove(_surround);
                        };
                    }
                    else if (e.PropertyName == FocusableSprite.HoverPropertyName)
                        action = () => _visual.Fill = _brushes[(Sprite.GetType(), true)];
                    else if (e.PropertyName == FocusableSprite.UnhoverPropertyName)
                        action = () => _visual.Fill = _brushes[(Sprite.GetType(), false)];

                    if (action != null)
                        Dispatcher.BeginInvoke(action);
                };
            }
        }

        private void SetControlDimensionsAndPosition()
        {
            MainCanvas.Width = Sprite.Surface.Width + (Sprite.Focused ? TotalStrokeSize : 0);
            MainCanvas.Height = Sprite.Surface.Height + (Sprite.Focused ? TotalStrokeSize : 0);
            SetValue(Canvas.LeftProperty, Sprite.Surface.Left - (Sprite.Focused ? StrokeAndSpace : 0));
            SetValue(Canvas.TopProperty, Sprite.Surface.Top - (Sprite.Focused ? StrokeAndSpace : 0));
            _visual.SetValue(Canvas.LeftProperty, Sprite.Focused ? StrokeAndSpace : double.NaN);
            _visual.SetValue(Canvas.TopProperty, Sprite.Focused ? StrokeAndSpace : double.NaN);
        }
    }
}
