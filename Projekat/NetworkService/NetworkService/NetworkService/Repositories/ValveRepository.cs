using NetworkService.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NetworkService.Repositories
{
    public class ValveRepository
    {
        private static ValveRepository _instance;
        private string valveFilePath = "C:\\Users\\Dimitrije\\Documents\\GitHub\\IUuIS_projekat2\\Projekat\\NetworkService\\NetworkService\\NetworkService\\public\\files\\Valves.txt";
        private int greatestId = 0;
        public static ValveRepository Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ValveRepository();
                return _instance;
            }
        }

        private ObservableCollection<Valve> valves;

        private ValveRepository()
        {
            valves = new ObservableCollection<Valve>();

            string[] lines = FileAccessManager.ReadFromFile(valveFilePath);

            foreach (string line in lines)
            {

                string[] splits = line.Split(',');
                int id = Convert.ToInt32(splits[0]);
                string name = splits[1];
                ValveType type = splits[2] == "CableSensor" ? ValveType.CableSensor : ValveType.DigitalManometer;
                int value = Convert.ToInt32(splits[3]);
                string dt = splits[4];

                valves.Add(new Valve(id, name, type, value, dt));
            }

            this.greatestId = valves.Any() ? valves.Max(v => v.Id) : 0;
        }

        public ObservableCollection<Valve> Valves { get { return valves; } }
        public int GreatestId { get { return greatestId; } set { this.greatestId = value; } }

        public void StoreValves()
        {
            //valve: Id,Name,Type,Value,DateTime\r\n


            File.WriteAllText(valveFilePath, string.Empty);
            int id = 1; //after closing, shifting all ids to stay in the sequence
                        //i.e. if valve with id == 3 is deleted, the sequence will have hole in that id,
                        //and, the more important, the latest item will not be simulated (beacuse his id > items.count)
            foreach (Valve v in valves)
                FileAccessManager.WriteToFile(valveFilePath, v.ToString(id++));
        }
    }

}
