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

        private List<object[]> _colorParts = new List<object[]>();

        public LoadingBar(double minValue, double maxValue, int segments = 10, ConsoleColor defaultColor = ConsoleColor.Gray)
        {
            _currentValue = minValue;
            _minValue = minValue;
            _maxValue = maxValue;
            _segments = segments;

            _topOffset = Console.CursorTop;
            
            if(defaultColor != ConsoleColor.Gray)
                _colorParts.Add(new object[] { 0, defaultColor });

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

        public void SetMaxValue(double newValue)
        {
            if (newValue > _minValue)
                _maxValue = newValue;
        }

        public void AddColor(ConsoleColor color)
        {
            double t = MathExtension.InverseLerp(_minValue, _maxValue, _currentValue);
            int count = (int)MathExtension.Lerp(0, _segments, t);

            _colorParts.Add(new object[] { count, color });
        }

        private void Render()
        {
            int[] offset = new int[2] { Console.CursorLeft, Console.CursorTop };

            Write(new string(' ', _segments + 8), 0, 0);
            Write("[", 0, 0);
            Write("]", _segments + 1, 1);

            double t = MathExtension.InverseLerp(_minValue, _maxValue, _currentValue);
            int count = (int)MathExtension.Lerp(0, _segments, t);

            Write($"{(int)(t * 100)}%", _segments + 3, 1);
            Write(new string('=', _colorParts.Count == 0 ? count : (int)_colorParts[0][0]), 1, 1);

            for(int i = 0; i < _colorParts.Count; i++)
            {
                int startIndex = (int)_colorParts[i][0];
                ConsoleColor color = (ConsoleColor)_colorParts[i][1];
                int length = 0;

                if (i < _colorParts.Count - 1)
                    length = (int)_colorParts[i + 1][0];
                else
                    length = count - startIndex;

                Console.ForegroundColor = color;
                Write(new string('=', length), 1 + startIndex, 1);
            }
            

            Console.SetCursorPosition(offset[0], offset[1]);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void Write(string value, int start, int end)
        {
            Console.SetCursorPosition(start, _topOffset);
            Console.Write(value);
            Console.SetCursorPosition(end, _topOffset);
        }
    }
}
