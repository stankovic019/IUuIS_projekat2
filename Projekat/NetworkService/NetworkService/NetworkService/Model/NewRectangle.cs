using NetworkService.Helpers;
using System.Windows.Media;

namespace NetworkService.Model
{
    public class NewRectangle : BindableBase
    {

        private Brush fill;

        public Brush Fill
        {
            get => fill;
            set => SetProperty(ref fill, value);
        }

    }

}
