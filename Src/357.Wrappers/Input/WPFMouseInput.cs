using Microsoft.Xna.Framework.Input;
using System;
using System.Windows;
using System.Windows.Input;

namespace Engine.Wrappers.Input
{
    /// <summary>
    /// Helper class that converts WPF mouse input to the XNA/MonoGame <see cref="MouseState"/>.
    /// </summary>
    public class WPFMouseInput
    {
        private readonly IInputElement _focusElement;

        private MouseState _mouseState;

        /// <summary>
        /// The current mousestate.
        /// </summary>
        public MouseState MouseState
        {
            get { return _mouseState; }
        }

        /// <summary>
        /// Creates a new instance of the keyboard helper.
        /// </summary>
        /// <param name="focusElement">The element that will be used as the focus point. Only if this element is correctly focused, mouse events will be handled.</param>
        public WPFMouseInput(IInputElement focusElement)
        {
            _focusElement = focusElement;
        }

        /// <summary>
        /// Handles all button related events.
        /// </summary>
        /// <param name="e">If <see cref="e.Handled"/> is true, it will be ignored, otherwise if the reference element is focused, the event will be handled and <see cref="e.Handled"/> will be set to true.</param>
        public void HandleMouseButtons(MouseButtonEventArgs e)
        {
            if (e.Handled || !_focusElement.IsMouseDirectlyOver)
                return;

            e.Handled = true;
            var conv =
                new Func<MouseButtonState, ButtonState>(
                    state => state == MouseButtonState.Pressed ? ButtonState.Pressed : ButtonState.Released);

            var m = _mouseState;
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _mouseState = new MouseState(m.X, m.Y, m.ScrollWheelValue, conv(e.ButtonState), m.MiddleButton, m.RightButton, m.XButton1, m.XButton2);
                    break;
                case MouseButton.Middle:
                    _mouseState = new MouseState(m.X, m.Y, m.ScrollWheelValue, m.LeftButton, conv(e.ButtonState), m.RightButton, m.XButton1, m.XButton2);
                    break;
                case MouseButton.Right:
                    _mouseState = new MouseState(m.X, m.Y, m.ScrollWheelValue, m.LeftButton, m.MiddleButton, conv(e.ButtonState), m.XButton1, m.XButton2);
                    break;
                // this code works, but the MonoGame MouseState class doesn't care for XButton1/XButton2 state - the properties always return ButtonState.Released
                case MouseButton.XButton1:
                    _mouseState = new MouseState(m.X, m.Y, m.ScrollWheelValue, m.LeftButton, m.MiddleButton, m.RightButton, conv(e.ButtonState), m.XButton2);
                    break;
                case MouseButton.XButton2:
                    _mouseState = new MouseState(m.X, m.Y, m.ScrollWheelValue, m.LeftButton, m.MiddleButton, m.RightButton, m.XButton1, conv(e.ButtonState));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Handles all move related events.
        /// </summary>
        /// <param name="e">If <see cref="e.Handled"/> is true, it will be ignored, otherwise if the reference element is focused, the event will be handled and <see cref="e.Handled"/> will be set to true.</param>
        public void HandleMouseMove(MouseEventArgs e)
        {
            if (e.Handled || !_focusElement.IsMouseDirectlyOver)
                return;

            e.Handled = true;
            var m = _mouseState;
            var pos = e.GetPosition(_focusElement);
            _mouseState = new MouseState((int)pos.X, (int)pos.Y, m.ScrollWheelValue, m.LeftButton, m.MiddleButton, m.RightButton, m.XButton1, m.XButton2);
        }

        /// <summary>
        /// Handles all button related events.
        /// </summary>
        /// <param name="e">If <see cref="e.Handled"/> is true, it will be ignored, otherwise if the reference element is focused, the event will be handled and <see cref="e.Handled"/> will be set to true.</param>
        public void HandleMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Handled || !_focusElement.IsMouseDirectlyOver)
                return;

            e.Handled = true;
            var m = _mouseState;
            _mouseState = new MouseState(m.X, m.Y, e.Delta + m.ScrollWheelValue, m.LeftButton, m.MiddleButton, m.RightButton, m.XButton1, m.XButton2);
        }
    }
}