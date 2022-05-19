using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game_Xamarin.Persistence;
using System.IO;
using Xamarin.Forms;
using Xamarin.Essentials;

[assembly: Dependency(typeof(Game_Xamarin.Droid.Persistence.AndroidDataAccess))]
namespace Game_Xamarin.Droid.Persistence
{
    public class AndroidDataAccess : IGameDataAccess
    {

        public async Task<GameInfos> LoadAsync(String path)
        {
            using (var stream = await FileSystem.OpenAppPackageFileAsync(path))
            {
                try
                {
                    using (var reader = new StreamReader(stream))
                    {
                        List<Coord> trees = new List<Coord>();
                        List<Coord> baskets = new List<Coord>();
                        List<Hunter> hunters = new List<Hunter>();
                        String line = await reader.ReadLineAsync();
                        int tableSize = Convert.ToInt32(line);
                        line = await reader.ReadLineAsync();
                        double elapsedSeconds = Convert.ToDouble(line);
                        line = await reader.ReadLineAsync();
                        string[] splitted = line.Split(",");
                        int playerX = Convert.ToInt32(splitted[0]);
                        int playerY = Convert.ToInt32(splitted[1]);

                        line = await reader.ReadLineAsync();
                        splitted = line.Split(";");
                        foreach (string coord in splitted)
                        {
                            string[] splitCoord = coord.Split(",");
                            Coord treeCoord = new Coord(Int32.Parse(splitCoord[0]), Int32.Parse(splitCoord[1]));
                            trees.Add(treeCoord);
                        }

                        line = await reader.ReadLineAsync();
                        splitted = line.Split(";");
                        foreach (string coord in splitted)
                        {
                            string[] splitCoord = coord.Split(",");
                            Hunter hunter = null;
                            switch (splitCoord[2])
                            {
                                case "Up":
                                    hunter = new Hunter(Int32.Parse(splitCoord[0]), Int32.Parse(splitCoord[1]), Direction.Up);
                                    break;
                                case "Down":
                                    hunter = new Hunter(Int32.Parse(splitCoord[0]), Int32.Parse(splitCoord[1]), Direction.Down);
                                    break;
                                case "Left":
                                    hunter = new Hunter(Int32.Parse(splitCoord[0]), Int32.Parse(splitCoord[1]), Direction.Left);
                                    break;
                                case "Right":
                                    hunter = new Hunter(Int32.Parse(splitCoord[0]), Int32.Parse(splitCoord[1]), Direction.Right);
                                    break;
                            }
                            hunters.Add(hunter);
                        }

                        line = await reader.ReadLineAsync();
                        splitted = line.Split(";");
                        foreach (string coord in splitted)
                        {
                            string[] splitCoord = coord.Split(",");
                            Coord basketCoord = new Coord(Int32.Parse(splitCoord[0]), Int32.Parse(splitCoord[1]));
                            baskets.Add(basketCoord);
                        }

                        return new GameInfos(baskets, trees, hunters, tableSize, new Coord(playerX, playerY), elapsedSeconds);
                    }
                }
                catch
                {
                    throw new Exception("File not found!");
                }
                
            }
        }
    }
}