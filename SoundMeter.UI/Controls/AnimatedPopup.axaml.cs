using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using SoundMeter.UI.Services;
using System;
using System.Collections.Generic;

namespace SoundMeter.UI.Controls
{
    internal class AnimatedPopup : ContentControl
    {
        #region AvaloniaPropeties
        public static readonly DirectProperty<AnimatedPopup, TimeSpan> AnimationDurationProperty =
            AvaloniaProperty.RegisterDirect<AnimatedPopup, TimeSpan>(nameof(AnimationDuration), x => x.AnimationDuration);
        private TimeSpan _animationDuration;
        public TimeSpan AnimationDuration
        {
            get => _animationDuration;
            set
            {
                SetAndRaise(AnimationDurationProperty, ref _animationDuration, value);
            }
        }

        public static readonly DirectProperty<AnimatedPopup, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<AnimatedPopup, bool>(nameof(IsOpen), x => x.IsOpen, (o, v) => o.IsOpen = v);

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value) return;
                SetAndRaise(IsOpenProperty, ref _isOpen, value);
                if (value)
                    CreateUndarlayControl();
                StartAnimation();
            }
        }

        public static readonly DirectProperty<AnimatedPopup, double> UnderlayOpacityProperty =
            AvaloniaProperty.RegisterDirect<AnimatedPopup, double>(nameof(UnderlayOpacity), x => x.UnderlayOpacity, (o, v) => o.UnderlayOpacity = v);

        private double _underlayOpacity = 0.5;
        public double UnderlayOpacity
        {
            get => _underlayOpacity;
            set
            {
                SetAndRaise(UnderlayOpacityProperty, ref _underlayOpacity, value);
            }
        }

        public static readonly AttachedProperty<string> WithNameProperty =
            AvaloniaProperty.RegisterAttached<AnimatedPopup, Control,string>("WithName");

        public static void SetWithName(AvaloniaObject avaloniaObject, string popupName)
        {
            avaloniaObject.SetValue(WithNameProperty, popupName);
        }
        public static string GetWithName(AvaloniaObject element)
        {
            return element.GetValue(WithNameProperty);
        }

        static AnimatedPopup()
        {
            WithNameProperty.Changed.AddClassHandler<Control>(HandleNameChanged);
            _underLayControl = new Border()
            {
                Background = Brushes.Black,
                ZIndex = 9,
                Opacity = 0,
            };
        }

        private static void HandleNameChanged(Control control, AvaloniaPropertyChangedEventArgs args)
        {
            var popupName = args.NewValue as string;
            if (_controlsRelatedTo.ContainsKey(popupName))
                throw new InvalidOperationException("Popup already exists");
            _controlsRelatedTo.Add(popupName, control);
        }
        #endregion

        private Size _containerSize;
        private Size _previousContainerSize;
        private double _currentTick = 0.0;
        private bool _isSizeFind;
        private bool _isContentResizeAnimationInProgress;
        private bool _isAnimationInProgress;
        private readonly DispatcherTimer _timer;
        private static Control _underLayControl;

        private double TotalTicks => Math.Ceiling(AnimationDuration.TotalSeconds / FrameRate.TotalSeconds);
        private TimeSpan FrameRate => AppSettings.FrameRate;

        private static Dictionary<string, Control> _controlsRelatedTo = new();

        private Grid ParentGrid
        {
            get
            {
                if (Parent is not Grid grid)
                    throw new InvalidOperationException("Parent control must be Grid");
                return grid;
            }
        }
        public AnimatedPopup()
        {
            _timer = new DispatcherTimer() { Interval = FrameRate };
            _timer.Tick += (s, e) => UpdateTick();
            ///Could be many subscribers
            _underLayControl.PointerPressed += (s, e) =>
            {
                if (!IsOpen) return;
                IsOpen = false;
            };

            var stateManager = ServiceLocator.GetRequiredSerivce<IPopupStateChanged>();

            stateManager.OnActionCallback = (popupCommand) =>
            {
                var nextState = popupCommand.State switch
                {
                    PopupState.Open => Name == popupCommand.Name ? true : false,
                    PopupState.Close => Name == popupCommand.Name ? false : true
                };
                IsOpen = nextState;
            };
        }

        private void UpdateTick()
        {
            if(IsOpen)
                OpenTick();
            else 
                CloseTick();
        }

        private void StartAnimation()
        {
            if (_isAnimationInProgress || !_isSizeFind)
                return;

            if (IsOpen)
                _currentTick = 0.0;
            else
                _currentTick = TotalTicks;
            _isAnimationInProgress = true;
            _timer.Start();
        }
       
        private void OpenTick()
        {
            if (_currentTick > TotalTicks)
            {
                Width = double.NaN;
                Height = double.NaN;
                _timer.Stop();
                _isAnimationInProgress = false;
                return;
            }

            UpdateUI();
            _currentTick++;
        }

        private void CloseTick()
        {
            if (_currentTick <= 0)
            {
                ResetControlProperties();
                RemoveUnderlayControl();
                _timer.Stop();
                _isAnimationInProgress = false;
                return;
            }

            UpdateUI();
            _currentTick--;
        }

        private void CreateUndarlayControl()
        {
            _underLayControl.IsVisible = true;
            ParentGrid.Children.Insert(0, _underLayControl);

            if (ParentGrid.RowDefinitions.Count > 0)
                _underLayControl.SetValue(Grid.RowSpanProperty, ParentGrid.RowDefinitions.Count);
            if (ParentGrid.ColumnDefinitions.Count > 0)
                _underLayControl.SetValue(Grid.ColumnSpanProperty, ParentGrid.ColumnDefinitions.Count);
        }

        private void RemoveUnderlayControl()
        {
            if (Parent is Grid grid && grid.Children.Contains(_underLayControl))
            {
                grid.Children.Remove(_underLayControl);
            }
        }
        private void ResetControlProperties()
        {
            Width = 0;
            Height = 0;
            _underLayControl.Opacity = 0;
        }

        private void UpdateUI()
        {
            var ticksPercentage = _currentTick / TotalTicks;
            Width = _containerSize.Width * ticksPercentage;
            Height = _containerSize.Height * ticksPercentage;
            _underLayControl.Opacity = UnderlayOpacity * ticksPercentage;
        }

        public override void Render(DrawingContext context)
        {
            //If content size updated - animate
            if (_isSizeFind && IsOpen && !_isAnimationInProgress && !_isContentResizeAnimationInProgress)
            {
                _previousContainerSize = _containerSize;
                _containerSize = DesiredSize - Margin;
            }

            if (!_isSizeFind)
            {
                _isSizeFind = true;
                _containerSize = DesiredSize - Margin;
                if (!_controlsRelatedTo.TryGetValue(Name ?? string.Empty, out var control))
                    throw new InvalidOperationException("Popup doesnt related to control. Use AnimatedPopup.WithName property");
                var point = control.TranslatePoint(new Point(), ParentGrid) ?? throw new System.Exception();
                Dispatcher.UIThread.Post(() =>
                {
                    Height = 0;
                    Width = 0;
                    Margin = new Thickness(point.X, 0, 0, ParentGrid.Bounds.Height - point.Y);
                    if (ParentGrid.RowDefinitions.Count > 0)
                        SetValue(Grid.RowSpanProperty, ParentGrid.RowDefinitions.Count);
                    if (ParentGrid.ColumnDefinitions.Count > 0)
                        SetValue(Grid.ColumnSpanProperty, ParentGrid.ColumnDefinitions.Count);
                });
            }
           base.Render(context);
        }
    }
}
