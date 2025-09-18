using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NetworkService.Repositories
{
    public class ValveRepository
    {
        private static ValveRepository _instance;
        private string valveFilePath = "C:\\Users\\Dimitrije\\Documents\\GitHub\\IUuIS_projekat2\\Projekat\\NetworkService\\NetworkService\\NetworkService\\public\\files\\Valves.txt";
        public static ValveRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ValveRepository();
                return _instance;
            }
        }

        private readonly ObservableCollection<Valve> valves;

        private ValveRepository()
        {
            valves = new ObservableCollection<Valve>();

            string[] lines = FileAccessManager.ReadFromFile(valveFilePath);

            foreach (string line in lines) {

                string[] splits = line.Split(',');
                int id = Convert.ToInt32(splits[0]);
                string name = splits[1];
                ValveType type = splits[2] == "CableSensor" ? ValveType.CableSensor : ValveType.DigitalManometer;
                int value = Convert.ToInt32(splits[3]);
                string dt = splits[4];

                valves.Add(new Valve(id, name, type, value, dt));
            }

        }

        public ObservableCollection<Valve> Valves { get { return valves; } }

        public void StoreValves()
        {
            //valve: Id,Name,Type,Value,DateTime\r\n
            
           
            File.WriteAllText(valveFilePath, string.Empty);

            foreach (Valve v in valves)
                FileAccessManager.WriteToFile(valveFilePath, v.ToString());
        }
    }

}
