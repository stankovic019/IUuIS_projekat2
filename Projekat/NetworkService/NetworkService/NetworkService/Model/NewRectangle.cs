using NetworkService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetworkService.Model
{
    public class NewRectangle : BindableBase
    {

        private Brush fill;

        public Brush Fill 
        { get => fill;
          set => SetProperty(ref fill, value); 
        }

    }

}
