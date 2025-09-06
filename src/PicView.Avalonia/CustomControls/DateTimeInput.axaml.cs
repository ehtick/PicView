using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace PicView.Avalonia.CustomControls;

/// <summary>
/// A custom control for date and time input, composed of separate fields
/// for year, month, day, hour, and minute. The order of the fields is
/// determined by the current culture's date and time formats.
/// </summary>
public class DateTimeInput : TemplatedControl
{
    // Flag to prevent recursive updates between the main property and the text boxes.
    private bool _isUpdatingFromProperty;

    // Holds the TextBoxes for each part of the DateTime.
    private TextBox? _yearBox, _monthBox, _dayBox, _hourBox, _minuteBox;

    /// <summary>
    /// Static constructor to register the default style for this control.
    /// </summary>
    static DateTimeInput()
    {
        // When the SelectedDateTime property changes, update the internal TextBox values.
        SelectedDateTimeProperty.Changed.AddClassHandler<DateTimeInput>((x, e) => x.OnSelectedDateTimeChanged(e));
    }


    /// <summary>
    /// Called when the control's template is applied. This is where we find
    /// the container panel and dynamically add our input fields.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Find the container that will hold our dynamic controls.
        var container = e.NameScope.Find<Panel>("PART_Container");
        if (container == null)
        {
            // The template must have a panel named PART_Container.
            throw new InvalidOperationException("Could not find PART_Container in the control template.");
        }

        // Generate and add the date/time input controls based on current culture.
        BuildInputControls(container);

        // Set initial values if SelectedDateTime is already set.
        UpdateTextBoxesFromDateTime(SelectedDateTime);
    }

    /// <summary>
    /// Handles changes to the SelectedDateTime property, updating the UI.
    /// </summary>
    private void OnSelectedDateTimeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Set a flag to indicate that the update is coming from the property,
        // not from user input in the TextBoxes.
        _isUpdatingFromProperty = true;
        UpdateTextBoxesFromDateTime(e.NewValue as DateTime?);
        _isUpdatingFromProperty = false;
    }

    /// <summary>
    /// Populates the container panel with TextBox and separator controls
    /// in an order determined by the current culture.
    /// </summary>
    /// <param name="container">The panel to add controls to.</param>
    private void BuildInputControls(Panel container)
    {
        container.Children.Clear();
        var culture = CultureInfo.CurrentCulture;
        var dateTimeFormat = culture.DateTimeFormat;

        // --- Create TextBoxes ---
        _yearBox = CreateNumericTextBox(4, "YYYY");
        _monthBox = CreateNumericTextBox(2, "MM");
        _dayBox = CreateNumericTextBox(2, "DD");
        _hourBox = CreateNumericTextBox(2, "HH");
        _minuteBox = CreateNumericTextBox(2, "mm");

        // --- Determine Date Field Order ---
        var dateParts = dateTimeFormat.ShortDatePattern.Split(dateTimeFormat.DateSeparator)
            .Select(p => p.ToLowerInvariant().Trim(' '));

        var dateControls = new List<Control>();
        foreach (var part in dateParts)
        {
            if (part.Contains('y') && _yearBox != null)
            {
                dateControls.Add(_yearBox);
            }
            else if (part.Contains('m') && _monthBox != null)
            {
                dateControls.Add(_monthBox);
            }
            else if (part.Contains('d') && _dayBox != null)
            {
                dateControls.Add(_dayBox);
            }
        }

        // --- Add Date Controls to Container ---
        for (var i = 0; i < dateControls.Count; i++)
        {
            container.Children.Add(dateControls[i]);
            if (i < dateControls.Count - 1)
            {
                container.Children.Add(CreateLineSeparator('/'));
            }
        }

        // --- Add Time Controls to Container ---
        // Simplified time order (Hour then Minute is almost universal)
        container.Children.Add(CreateLineSeparator('—'));
        container.Children.Add(_hourBox);
        container.Children.Add(CreateTimeSeparator());
        container.Children.Add(_minuteBox);
    }

    /// <summary>
    /// Creates a TextBox configured for numeric input.
    /// </summary>
    private NumTextBox CreateNumericTextBox(int maxLength, string watermark)
    {
        var textBox = new NumTextBox
        {
            MaxLength = maxLength,
            Watermark = watermark,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(2,0),
            FontFamily = new FontFamily("avares://PicView.Avalonia/Assets/Fonts/Roboto-Regular.ttf#Roboto")
        };
        textBox.TextChanged += OnPartTextChanged;
        return textBox;
    }

    /// <summary>
    /// Creates a separator control.
    /// </summary>
    private static TextBlock CreateTimeSeparator()
    {
        return new TextBlock
        {
            Classes = { "txt" },
            Text = ":",
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
    
    private static TextBlock CreateLineSeparator(char separatorChar)
    {
        return new TextBlock
        {
            Classes = { "txt" },
            Text = $" {separatorChar} ",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 18,
            Opacity = .6
        };
    }

    /// <summary>
    /// Event handler for when text changes in any of the input TextBoxes.
    /// It attempts to parse the current input into a DateTime object.
    /// </summary>
    private void OnPartTextChanged(object? sender, TextChangedEventArgs e)
    {
        // If the text is being changed by our own code, do nothing.
        if (_isUpdatingFromProperty)
        {
            return;
        }

        // Try to parse the values from the text boxes into integers.
        var yearParsed = int.TryParse(_yearBox?.Text, out var year);
        var monthParsed = int.TryParse(_monthBox?.Text, out var month);
        var dayParsed = int.TryParse(_dayBox?.Text, out var day);
        var hourParsed = int.TryParse(_hourBox?.Text, out var hour);
        var minuteParsed = int.TryParse(_minuteBox?.Text, out var minute);

        // Only update the source property if all fields have a valid number.
        if (yearParsed && monthParsed && dayParsed && hourParsed && minuteParsed)
        {
            try
            {
                // Attempt to create a valid DateTime object.
                var newDateTime = new DateTime(year, month, day, hour, minute, 0);
                SetCurrentValue(SelectedDateTimeProperty, newDateTime);
            }
            catch (ArgumentOutOfRangeException)
            {
                // One of the values is out of range (e.g., month 13).
                // In a real app, you might add validation feedback here.
                SetCurrentValue(SelectedDateTimeProperty, null);
            }
        }
        else
        {
            // If any part is not a valid number, the overall DateTime is invalid.
            SetCurrentValue(SelectedDateTimeProperty, null);
        }
    }

    /// <summary>
    /// Updates the text in the individual TextBoxes based on the provided DateTime.
    /// </summary>
    private void UpdateTextBoxesFromDateTime(DateTime? dt)
    {
        if (_yearBox == null)
        {
            return; // Controls not ready yet.
        }

        if (dt.HasValue)
        {
            _yearBox.Text = dt.Value.Year.ToString("D4");
            _monthBox!.Text = dt.Value.Month.ToString("D2");
            _dayBox!.Text = dt.Value.Day.ToString("D2");
            _hourBox!.Text = dt.Value.Hour.ToString("D2");
            _minuteBox!.Text = dt.Value.Minute.ToString("D2");
        }
        else
        {
            // If the DateTime is null, clear all text boxes.
            _yearBox.Text = string.Empty;
            _monthBox!.Text = string.Empty;
            _dayBox!.Text = string.Empty;
            _hourBox!.Text = string.Empty;
            _minuteBox!.Text = string.Empty;
        }
    }

    #region Avalonia Properties

    /// <summary>
    /// Defines the SelectedDateTime dependency property.
    /// </summary>
    public static readonly StyledProperty<DateTime?> SelectedDateTimeProperty =
        AvaloniaProperty.Register<DateTimeInput, DateTime?>(
            nameof(SelectedDateTime),
            null,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets the selected date and time.
    /// This is the main property to bind to from your ViewModel.
    /// </summary>
    public DateTime? SelectedDateTime
    {
        get => GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    #endregion
}