using System.Windows;
using System.Windows.Controls;

namespace Cider_x64
{
    class PlayStopButton : CheckBox
    {
        static PlayStopButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayStopButton), new FrameworkPropertyMetadata(typeof(PlayStopButton)));
        }
    }
}
