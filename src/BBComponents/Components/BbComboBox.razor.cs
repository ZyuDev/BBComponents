﻿using BBComponents.Enums;
using BBComponents.Helpers;
using BBComponents.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBComponents.Components
{
    public partial class BbComboBox<TValue>: ComponentBase
    {

        private string _inputValue;
        private bool _isOpen;
        private bool _stopListenOnInputValueChange;
        private List<SelectItem<TValue>> _source = new List<SelectItem<TValue>>();

        [Parameter]
        public BootstrapElementSizes Size { get; set; }

        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public TValue Value { get; set; }

        /// <summary>
        /// Event call back for value changed.
        /// </summary>
        [Parameter]
        public EventCallback<TValue> ValueChanged { get; set; }

        /// <summary>
        /// Duplicate event call back for value changed. 
        /// It is necessary to have possibility catch changed even whet we use @bind-Value.
        /// </summary>
        [Parameter]
        public EventCallback<TValue> Changed { get; set; }

        /// <summary>
        /// Colllection for select options.
        /// </summary>
        [Parameter]
        public IEnumerable<object> ItemsSource { get; set; }

        /// <summary>
        /// Name of property with option Value.
        /// </summary>
        [Parameter]
        public string ValueName { get; set; }

        /// <summary>
        /// Name of property with option IsDeleted.
        /// </summary>
        [Parameter]
        public string IsDeletedName { get; set; }

        /// <summary>
        /// Name of property with option Text.
        /// </summary>
        [Parameter]
        public string TextName { get; set; }

        public string SizeClass => HtmlClassBuilder.BuildSizeClass("input-group", Size);

        public string TopDrowdown
        {
            get
            {
                int topValue;
                switch (Size)
                {
                    case BootstrapElementSizes.Sm:
                        topValue = 32;
                        break;
                    case BootstrapElementSizes.Lg:
                        topValue = 49;
                        break;
                    default:
                        topValue = 39;
                        break;
                }

                return $"{topValue}px";
            }
        }

        public List<SelectItem<TValue>> SourceFiltered
        {
            get
            {
                var sourceFiltered = new List<SelectItem<TValue>>();

                if (string.IsNullOrEmpty(_inputValue))
                {
                    sourceFiltered = _source;
                }
                else
                {
                    var searchString = _inputValue.ToLower().Trim();
                    var parts = searchString.Split(' ');
                    
                    foreach(var item in _source)
                    {
                        if (string.IsNullOrWhiteSpace(item.Text))
                        {
                            continue;
                        }

                        var flagAdd = true;
                        foreach(var part in parts)
                        {
                            if (!item.Text.ToLower().Contains(part))
                            {
                                flagAdd = false;
                                break;
                            }
                        }

                        if (flagAdd)
                        {
                            sourceFiltered.Add(item);
                        }
                    }

                }

                return sourceFiltered;
            }
        }

        protected override void OnParametersSet()
        {
            _source = new List<SelectItem<TValue>>();

            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    var propValue = item.GetType().GetProperty(ValueName);
                    var propText = item.GetType().GetProperty(TextName);

                    var value = (TValue) propValue?.GetValue(item);
                    var text = propText?.GetValue(item)?.ToString();

                    var isDeleted = false;

                    if (!string.IsNullOrEmpty(IsDeletedName))
                    {
                        var propIsDeleted = item.GetType().GetProperty(IsDeletedName);
                        var isDeletedValue = propIsDeleted?.GetValue(item);
                        isDeleted = isDeletedValue == null ? false : (bool)isDeletedValue;
                    }

                    _source.Add(new SelectItem<TValue>(text,value, isDeleted));
                }
            }
        }

        private void OnOpenClick()
        {
            _isOpen = !_isOpen;
        }

        private async Task OnClearClick()
        {
            _inputValue = "";

            var defaultValue = default(TValue);
            await ValueChanged.InvokeAsync(defaultValue);
            await Changed.InvokeAsync(defaultValue);


        }

        private void OnInputValueChange(ChangeEventArgs e)
        {
            if (_stopListenOnInputValueChange)
            {
                _stopListenOnInputValueChange = false;
                return;
            }

            _inputValue = e.Value?.ToString();
            Console.WriteLine($"InputValueChange {_inputValue}");
        }

        private void OnInput(ChangeEventArgs e)
        {
            _inputValue = e.Value?.ToString();

            Console.WriteLine($"InputValue {_inputValue}");


            if (!_isOpen)
            {
                _isOpen = true;
            }
        }

        private async Task OnInputKeyPress(KeyboardEventArgs e)
        {
            if (e.Code == "Enter")
            {
                if (SourceFiltered.Count == 1)
                {
                    _stopListenOnInputValueChange = true;

                    var item = SourceFiltered[0];

                    _inputValue = item.Text;
                    StateHasChanged();

                    Console.WriteLine($"KeyPress {_inputValue}");


                    await ValueChanged.InvokeAsync(item.Value);
                    await Changed.InvokeAsync(item.Value);
                    _isOpen = false;

                }
            }
        }

        private async Task OnItemClick(MouseEventArgs e, SelectItem<TValue> item)
        {
            _inputValue = item.Text;
            await ValueChanged.InvokeAsync(item.Value);
            await Changed.InvokeAsync(item.Value);
            _isOpen = false;
        }
    }
}