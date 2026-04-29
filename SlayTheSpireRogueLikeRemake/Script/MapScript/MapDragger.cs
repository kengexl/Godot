using Godot;

namespace YourNamespace
{
    [GlobalClass]
    public partial class MapDragger : Control
    {
        private bool _isDragging = false;
        private Vector2 _dragStartMousePos;
        private Vector2 _dragStartScrollPos;

        public override void _GuiInput(InputEvent @event)
        {
            // 只处理鼠标左键事件
            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    // 开始拖动
                    _isDragging = true;
                    _dragStartMousePos = mb.GlobalPosition;
                    _dragStartScrollPos = new Vector2(
                        GetParent<ScrollContainer>().ScrollHorizontal,
                        GetParent<ScrollContainer>().ScrollVertical
                    );
                }
                else
                {
                    // 结束拖动
                    _isDragging = false;
                }
            }

            // 处理拖动
            if (@event is InputEventMouseMotion mm && _isDragging)
            {
                ScrollContainer sc = GetParent<ScrollContainer>();
                Vector2 delta = mm.GlobalPosition - _dragStartMousePos;
                sc.ScrollVertical = (int)(_dragStartScrollPos.Y - delta.Y);
            }
        }
    }
}