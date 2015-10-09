
#include "stdafx.h"

using namespace System::Windows;
using namespace System::Windows::Controls;
using namespace System::Windows::Media;

namespace Ciderx64SampleGuiElements {

    /// <summary>
    /// Sample WPF GUI element (to be used for testing purposes only)
    /// </summary>
    public ref class SampleUserControlRed : public System::Windows::Controls::UserControl
    {
    public:
        SampleUserControlRed(void)
        {
            this->Background = gcnew SolidColorBrush(Colors::IndianRed);

            auto contentText = gcnew TextBlock();
            contentText->Text = "Sample UserControl";
            contentText->Foreground = gcnew SolidColorBrush(Colors::White);
            contentText->FontSize = 30;
            contentText->FontWeight = FontWeights::Black;
            contentText->HorizontalAlignment = System::Windows::HorizontalAlignment::Center;
            contentText->VerticalAlignment = System::Windows::VerticalAlignment::Center;
            this->Content = contentText;
        }
    };
}
