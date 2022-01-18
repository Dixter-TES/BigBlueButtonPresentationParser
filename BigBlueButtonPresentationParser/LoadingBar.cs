using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBlueButtonPresentationParser
{
    internal class LoadingBar
    {
        private double _currentValue;
        private int _topOffset;

        private double _minValue;
        private double _maxValue;
        private int _segments;

        private LoadingBarVisualization _visualization;

        private List<object[]> _colorParts = new List<object[]>();

        public LoadingBar(double minValue, double maxValue, int segments = 100, LoadingBarVisualization visualization = null)
        {
            _currentValue = minValue;
            _minValue = minValue;
            _maxValue = maxValue;
            _segments = segments;

            _topOffset = Console.CursorTop;
            
            if(visualization != null)
            {
                _visualization = visualization;
                _colorParts.Add(new object[] { 0, visualization.FillColor });
            }
            else
            {
                _visualization = LoadingBarVisualization.Standard;
                _colorParts.Add(new object[] { 0, LoadingBarVisualization.Standard.FillColor });
            }
                

            Render();
        }

        public void Update(double newValue)
        {
            _currentValue = newValue;
            Render();
        }

        public double GetValue()
        {
            return _currentValue;
        }

        public void SetMinValue(double newValue)
        {
            if (newValue < _maxValue)
                _minValue = newValue;
        }

        public void SetMaxValue(double newValue)
        {
            if (newValue > _minValue)
                _maxValue = newValue;
        }

        public void AddColor(ConsoleColor color, double percent = -1)
        {
            double t = MathExtension.InverseLerp(_minValue, _maxValue, _currentValue);
            int count = (int)MathExtension.Lerp(0, _segments, t);

            if((int)percent != -1)
            {
                count = (int)MathExtension.Lerp(0, _segments, MathExtension.InverseLerp(_minValue, _maxValue,
                    ((_maxValue - _minValue) * percent) + _minValue));
            }

            _colorParts.Add(new object[] { count, color });
        }

        private void Render()
        {
            int[] offset = new int[2] { Console.CursorLeft, Console.CursorTop };

            Write(new string(' ', _segments + 8), 0, 0);
            Write(_visualization.StartBorderChar, 0, 0, _visualization.BorderColor);
            Write(_visualization.EndBorderChar, _segments + 1, 1, _visualization.BorderColor);

            double t = MathExtension.InverseLerp(_minValue, _maxValue, _currentValue);
            int count = (int)MathExtension.Lerp(0, _segments, t);

            Write($"{(int)(t * 100)}%", _segments + 3, 1);
            Write(new string(_visualization.FillChar, _colorParts.Count == 0 ? count : (int)_colorParts[0][0]), 1, 1);

            for (int i = 0; i < _colorParts.Count; i++)
            {
                int startIndex = (int)_colorParts[i][0];
                ConsoleColor color = (ConsoleColor)_colorParts[i][1];
                int length;

                if (i < _colorParts.Count - 1)
                    length = count - (int)_colorParts[i][0];
                else
                    length = count - startIndex;

                Write(new string(_visualization.FillChar, length < 0 ? 0 : length), 1 + startIndex, 1, color);
            }

            Console.SetCursorPosition(offset[0], offset[1]);
        }

        private void Write(string value, int start, int end, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.SetCursorPosition(start, _topOffset);
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = original;
            Console.SetCursorPosition(end, _topOffset);
        }

        private void Write(char value, int start, int end, ConsoleColor color = ConsoleColor.Gray) => Write(value.ToString(), start, end, color);
    }

    internal class LoadingBarVisualization
    {
        public static LoadingBarVisualization Standard = new LoadingBarVisualization();

        private char _fillChar = '=';
        private char _startBorderChar = '[';
        private char _endBorderChar = ']';

        private ConsoleColor _fillColor = ConsoleColor.White;
        private ConsoleColor _borderColor = ConsoleColor.Gray;

        public char FillChar {
            get => _fillChar;
            set => _fillChar = value;
        }

        public char StartBorderChar
        {
            get => _startBorderChar;
            set => _startBorderChar = value;
        }

        public char EndBorderChar
        {
            get => _endBorderChar;
            set => _endBorderChar = value;
        }

        public ConsoleColor FillColor
        {
            get => _fillColor;
            set => _fillColor = value;
        }

        public ConsoleColor BorderColor
        {
            get => _borderColor;
            set => _borderColor = value;
        }
    }

}
