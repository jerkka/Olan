using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GroupBox = System.Windows.Controls.GroupBox;
using TabControl = System.Windows.Controls.TabControl;

namespace Olan.UI {
    public static class Extensions {
        public static IEnumerable<ConfigurablePropertyAttribute> GetConfigurableProperties(this Type type) {
            return type.GetProperties().Select(pi => pi.GetCustomAttribute<ConfigurablePropertyAttribute>()?.SetPropertyInfo(pi)).Where(cpa => cpa != null).ToList();
        }
    }

    /// <summary>
    ///     Attribute to set as solution setting property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigurablePropertyAttribute : Attribute {
        #region Fields

        private string _category;
        private string _displayName;
        public object DefaultValue = null;
        public PropertyInfo PropertyInfo;
        public string Collection = null;
        public int CollectionItemIndex = 0;
        public int CollectionItemWidth = 0;
        public string Description = null;
        public double Frequency = 0d;
        public bool IsUserControl = false;
        public bool IsCollection = false;
        public bool IsCollectionItem = false;
        public bool IsTab = false;
        public bool IsTabItem = false;
        public double LimitHigh = 0d;
        public double LimitLow = 0d;
        public string TabName = null;

        #endregion
        #region Properties

        public string DisplayName {
            get { return _displayName ?? (_displayName = PropertyInfo?.Name); }
            set { _displayName = value; }
        }

        public string Category {
            get { return _category ?? (_category = PropertyInfo?.PropertyType.Name ?? "Common"); }
            set { _category = value; }
        }

        #endregion
        #region Methods

        public ConfigurablePropertyAttribute SetPropertyInfo(PropertyInfo pi) {
            PropertyInfo = pi;
            return this;
        }

        public UIElement GetUIElement(object source) {
            if (PropertyInfo == null) {
                throw new ArgumentNullException(nameof(PropertyInfo));
            }
            if (IsUserControl || IsTabItem) {
                return new AutoUserControl(PropertyInfo.GetValue(source, null));
            }
            if (IsTab) {
                return new AutoTab(PropertyInfo.GetValue(source, null), this);
            }
            if (IsCollection) {
                
            }
            if (PropertyInfo.GetValue(source, null) is ICommand) {
                return new AutoButton(source, PropertyInfo, this);
            }
            var propertyType = PropertyInfo.PropertyType;
            if (propertyType.GetCustomAttribute<FlagsAttribute>() != null) {
                return new AutoMultiSelectComboBox(source, PropertyInfo, this);
            }
            if (propertyType.IsEnum) {
                return new AutoComboBox(source, PropertyInfo, this);
            }
            switch (Type.GetTypeCode(propertyType)) {
                case TypeCode.Boolean:
                    return new AutoCheckBox(source, PropertyInfo, this);
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return new AutoTipedSlider(source, PropertyInfo, this);
                case TypeCode.String:
                    return new AutoTextBox(source, PropertyInfo, this);
                default:
                    throw new InvalidOperationException($"{Type.GetTypeCode(propertyType)} are not yet supported.");
            }
        }

        #endregion
    }

    public class AutoUserControl : StackPanel {
        #region Constructors

        public AutoUserControl(object source) {
            DataContext = source;
            var cpas = source.GetType().GetProperties().Select(pi => pi.GetCustomAttribute<ConfigurablePropertyAttribute>()?.SetPropertyInfo(pi)).Where(cpa => cpa != null).ToList();
            if (cpas.Count > 0) {
                if (cpas.Count > 1) {
                    var categories = new ConcurrentDictionary<string, StackPanel>();
                    foreach (var cpa in cpas) {
                        var category = categories.GetOrAdd(cpa.Category, s => new StackPanel());
                        category.Children.Add(cpa.GetUIElement(source));
                    }
                    var grid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    var count = 0;
                    foreach (var sp in categories.OrderBy(kv => kv.Key)) {
                        sp.Value.Margin = new Thickness(0, 3, 0, 0);
                        //var groupBox = new GroupBox {
                        //    Content = sp.Value,
                        //    Header = sp.Key,
                        //    Foreground = Brushes.WhiteSmoke,
                        //    Margin = new Thickness(3, 0, 3, 3)
                        //};
                        //grid.RowDefinitions.Add(new RowDefinition());
                        //Grid.SetRow(sp.Value, count);
                        grid.Children.Add(sp.Value);
                        count++;
                    }
                    Children.Add(grid);
                }
                else {
                    Children.Add(cpas.FirstOrDefault()?.GetUIElement(source));
                }
            }
        }

        #endregion
    }

    public class AutoTab : TabControl {
        public AutoTab(object source, ConfigurablePropertyAttribute cpa) {
            Name = cpa.DisplayName;
            var cpas = source.GetType().GetProperties().Select(pi => pi.GetCustomAttribute<ConfigurablePropertyAttribute>()?.SetPropertyInfo(pi)).Where(cp => cp != null && cp.IsTabItem).ToList();
            foreach (var subCpa in cpas) {
                Items.Add(new TabItem {
                    Name = subCpa.DisplayName,
                    Content = subCpa.GetUIElement(source)
                });
            }
        }
    }

    public class AutoElementPanel<TElement> : UniformGrid
        where TElement : UIElement {
        #region Fields

        protected static readonly Border Border = new Border {
            BorderThickness = new Thickness(1)
        };
        protected static readonly StackPanel ControlPanel = new StackPanel {
            Orientation = Orientation.Horizontal
        };
        protected static readonly UniformGrid Grid = new UniformGrid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns = 1,
            Background = Brushes.Blue
        };
        protected static readonly Label Label = new Label {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinWidth = 50,
            MinHeight = 25
        };
        protected static readonly StackPanel NamePanel = new StackPanel {
            Orientation = Orientation.Horizontal
        };
        protected static readonly ResetButton ResetButton = new ResetButton {
            Content = "Reset",
            Width = 50,
            Height = 25
        };
        protected readonly TElement Element;

        #endregion
        #region Constructors

        static AutoElementPanel() {
        }

        public AutoElementPanel(TElement element, object source, string name, string toolTip = null, bool useBorder = false) {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            Element = element;
            ToolTip = toolTip;
            DataContext = source;
            Label.Content = name;
            ResetButton.ResetAction = null;
            var grid = new UniformGrid {
                Columns = 2,
                Rows = 1
            };
            grid.Children.Add(ResetButton);
            grid.Children.Add(Label);
            Children.Add(grid);
            Children.Add(Element);
        }

        #endregion
        #region Methods

        protected Binding GenerateBinding(string xpath, PropertyInfo propertyInfo) {
            return new Binding(xpath) {
                Path = new PropertyPath(propertyInfo.Name), Mode = BindingMode.TwoWay
            };
        }

        #endregion
    }

    public class ResetButton : Button {
        #region Fields

        private Action _resetAction;

        #endregion
        #region Properties

        public Action ResetAction {
            get { return _resetAction; }
            set {
                _resetAction = value;
                if (_resetAction == null) {
                    Command = null;
                    IsEnabled = false;
                    return;
                }
                Command = new RelayCommand(_resetAction);
                IsEnabled = true;
            }
        }

        #endregion
    }

    public class MultiSelectComboBox : StackPanel {
        #region Fields

        protected static readonly FlagsEnumValueConverter Converter = new FlagsEnumValueConverter();

        #endregion
        #region Constructors

        public MultiSelectComboBox(object source, PropertyInfo propertyInfo) {
            var grid = new UniformGrid {

                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            var vals = Enum.GetValues(propertyInfo.PropertyType);
            for (var i = 0; i < vals.Length; i++) {
                var value = vals.GetValue(i);
                var checkBox = new CheckBox {
                    Content = value.ToString()
                };
                var binding = new Binding("IsChecked") {
                    Source = source, Path = new PropertyPath(propertyInfo.Name), Mode = BindingMode.TwoWay, Converter = Converter, ConverterParameter = value.ToString()
                };
                checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);
                //Grid.SetRow(checkBox, i);
                grid.Children.Add(checkBox);
            }
            Children.Add(grid);
        }

        #endregion
    }

    public class TipedSlider : StackPanel {
        #region Constructors

        public TipedSlider(object source, PropertyInfo propertyInfo, ConfigurablePropertyAttribute cpa) {
            Orientation = Orientation.Horizontal;
            var sliderLabel = new Label {
                Foreground = Brushes.WhiteSmoke, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center
            };
            var slider = new Slider {
                TickPlacement = TickPlacement.BottomRight, IsSnapToTickEnabled = true, SnapsToDevicePixels = true, Style = null
            };
            var labelBinding = new Binding("Value") {
                Source = slider, Path = new PropertyPath("Value"), Mode = BindingMode.TwoWay
            };
            var border = new Border {
                BorderThickness = new Thickness(1)
            };
            slider.Maximum = cpa.LimitHigh;
            slider.Minimum = cpa.LimitLow;
            slider.TickFrequency = cpa.Frequency > 0d ? cpa.Frequency : Math.Abs(cpa.LimitHigh - cpa.LimitLow)/100f;
            slider.SmallChange = slider.TickFrequency;
            slider.LargeChange = slider.TickFrequency*10f;
            slider.SetBinding(RangeBase.ValueProperty, GenerateBinding("Value", propertyInfo));
            sliderLabel.SetBinding(ContentControl.ContentProperty, labelBinding);
            sliderLabel.ContentStringFormat = "N2";
            border.Child = sliderLabel;
            DataContext = source;
            Children.Add(slider);
            Children.Add(border);
        }

        #endregion
        #region Methods

        protected Binding GenerateBinding(string xpath, PropertyInfo propertyInfo) {
            return new Binding(xpath) {
                Path = new PropertyPath(propertyInfo.Name), Mode = BindingMode.TwoWay
            };
        }

        #endregion
    }

    public class AutoButton : AutoElementPanel<Button> {
        #region Constructors

        public AutoButton(object source, PropertyInfo pi, ConfigurablePropertyAttribute cpa)
            : base(new Button {
                Content = cpa.DisplayName, Command = (ICommand)pi.GetValue(source, null)
            }, source, cpa.DisplayName, cpa.Description) { }

        #endregion
    }

    public class AutoCheckBox : AutoElementPanel<CheckBox> {
        #region Constructors

        public AutoCheckBox(object source, PropertyInfo pi, ConfigurablePropertyAttribute cpa)
            : base(new CheckBox {
            }, source, cpa.DisplayName, cpa.Description) {
            Element.SetBinding(ToggleButton.IsCheckedProperty, GenerateBinding("IsChecked", pi));
            if (cpa.DefaultValue != null) {
                ResetButton.ResetAction = () => Element.IsChecked = Convert.ToBoolean(cpa.DefaultValue);
            }
        }

        #endregion
    }

    public class AutoComboBox : AutoElementPanel<ComboBox> {
        #region Constructors

        public AutoComboBox(object source, PropertyInfo pi, ConfigurablePropertyAttribute cpa)
            : base(new ComboBox {
            }, source, cpa.DisplayName, cpa.Description, true) {
            foreach (var val in Enum.GetValues(pi.PropertyType)) {
                Element.Items.Add(val);
            }
            Element.SetBinding(Selector.SelectedItemProperty, GenerateBinding("SelectedItem", pi));
            if (cpa.DefaultValue != null) {
                ResetButton.ResetAction = () => Element.SelectedItem = cpa.DefaultValue;
            }
        }

        #endregion
    }

    public class AutoMultiSelectComboBox : AutoElementPanel<MultiSelectComboBox> {
        #region Constructors

        public AutoMultiSelectComboBox(object source, PropertyInfo pi, ConfigurablePropertyAttribute cpa)
            : base(new MultiSelectComboBox(source, pi), source, cpa.DisplayName, cpa.Description, true) {
            if (cpa.DefaultValue != null) {
                ResetButton.ResetAction = () => pi.SetValue(source, cpa.DefaultValue);
            }
        }

        #endregion
    }

    public class AutoTextBox : AutoElementPanel<TextBox> {
        #region Constructors

        public AutoTextBox(object source, PropertyInfo pi, ConfigurablePropertyAttribute cpa)
            : base(new TextBox {
            }, source, cpa.DisplayName, cpa.Description) {
            Element.SetBinding(ToggleButton.IsCheckedProperty, GenerateBinding("Text", pi));
            if (cpa.DefaultValue != null) {
                ResetButton.ResetAction = () => Element.Text = cpa.DefaultValue.ToString();
            }
        }

        #endregion
    }

    public class AutoTipedSlider : AutoElementPanel<TipedSlider> {
        #region Constructors

        public AutoTipedSlider(object source, PropertyInfo pi, ConfigurablePropertyAttribute cpa)
            : base(new TipedSlider(source, pi, cpa), source, cpa.DisplayName, cpa.Description) {
            if (cpa.DefaultValue != null) {
                ResetButton.ResetAction = () => pi.SetValue(source, cpa.DefaultValue);
            }
        }

        #endregion
    }
}