//using Zenject;
//using System;
//using UnityEngine;
//using Blizzard.Grid;
//using Unity.Burst.CompilerServices;

//namespace Blizzard.Temperature
//{
//    public class TemperatureGridCPU
//    {
//        public IDenseGrid<TemperatureCell> grid;

//        public TemperatureGridCPU(IDenseGrid<TemperatureCell> grid)
//        {
//            this.grid = grid;
//        }

//        public void UpdateTemperatureAll(float deltaTime, float ambientTemperature, bool doLog = false)
//        {
//            TemperatureCell?[] downRow = new TemperatureCell?[grid.Width]; // Row below the current row
//            Array.Fill(downRow, null);

//            TemperatureCell? leftCell = null; // Cell to the left of current cell

//            for (int y = 0; y < grid.Height; ++y)
//            {
//                for (int x = 0; x < grid.Width; ++x)
//                {
//                    if (x == 0) leftCell = null;

//                    //Debug.Log($"Cell at ({x}, {y})");
//                    TemperatureCell cell = grid.GetAt(x, y);
//                    grid.GetNeighborsAt(new Vector2Int(x, y), out TemperatureCell[] neighbors, leftCell, downRow[x]);
//                    downRow[x] = cell;
//                    leftCell = cell;

//                    DoCellHeatTransfer(ref cell, ref neighbors, deltaTime, ambientTemperature, 0); // TODO: get sourceHeat from somewhere

//                    grid.SetAt(x, y, cell);
//                }
//            }

//            if (doLog) grid.Log(); // TEMP (for testing)
//        }

//        /// <summary>
//        /// Calculates and applies heat transfer to given cell based on given cell neighbors, delta time, ambient temperature and cell source heat
//        /// </summary>
//        private void DoCellHeatTransfer(ref TemperatureCell cell, ref TemperatureCell[] neighbors, float deltaTime, float ambientTemperature, float sourceHeat)
//        {
//            float diffusionFactor = 0.2f; // temp (no hardcoding!)
//            float coolingFactor = 0f; // temp

//            float sumTemperatureDifference = 0f;
//            for (int i = 0; i < neighbors.Length; ++i)
//            {
//                sumTemperatureDifference += neighbors[i].temperature - cell.temperature;
//                //Debug.Log($"nbr temp: {neighbors[i].temperature}");
//            }

//            float diffusion = sumTemperatureDifference * diffusionFactor * deltaTime;
//            float ambientCooling = (cell.temperature - ambientTemperature) * coolingFactor * deltaTime;

//            float temperatureDelta = diffusion + ambientCooling + sourceHeat;
//            //Debug.Log($"temp delta: {temperatureDelta}");
//            cell.temperature += temperatureDelta;
//        }
//    }
//}

