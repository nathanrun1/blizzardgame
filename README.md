Currently working on a 2D topdown survival/factory building game. The game is set deep in a boreal forest as an unforgiving blizzard approaches.

Temperature is a core mechanic of this game, so I decided to start creating this game by building a dedicated system for it.

This is essentially a modified snapshot of my early in-development game. The temperature simulation scripts are found in [this folder,](https://github.com/nathanrun1/unity-temperature-diffusion/tree/main/Assets/Scripts/Systems/Temperature) and is built on top of the grid system created in [this folder.](https://github.com/nathanrun1/temperature-diffusion-sim/tree/main/Assets/Scripts/Systems/Grid)

The CPU-based temperature system attempt can be found in [this folder.](https://github.com/nathanrun1/temperature-diffusion-sim/tree/main/Assets/Scripts/Systems/Temperature/Old)

## Design of the Temperature System
The temperature system works by representing the temperature of each position in the world using a 2D grid. Each square in the grid represents a region (of customizable size) in the 2D scene that the player will build, fight and survive in. 
Each square holds a "cell", which contains three values:
- Temperature (Float of any value): The current temperature of the cell
- Insulation (Float in range 0 -> 1): How fast temperature can be transferred to/from the cell. Higher insulation means the cell changes temperature more slowly, insulation = 1 means it cannot transfer heat. This is useful for shelters built by the player, for example.
- Heat (Float of any value): How much temperature is added (or removed if negative) to this cell per second, useful for heat sources, or even negative heat sources, e.g. enemies that steal heat from the player's constructions by walking near them.

During runtime, heat is transferred between a cell and its four neighbors (i.e. not including diagonal neighbors), and also gained/lost based on the cell's heat.\
The temperature change due to heat transfer is calculated by adding the temperature difference between this cell and each of its neighbors, and then multiplying the sum by a 
set diffusion factor $\alpha$, the change in time since the last update $\Delta t$, and again by the complement of the insulation $1 - I$.\
The cell additionally gains/loses temperature based on its heat value, by adding the product of the cell's heat with the change in time $h\Delta t$\
As a result, the overall temperature change is calculated using the following formula:
$$\Delta T = \alpha \Delta t (1-I)\sum_{T_n \in N}(T-T_n) + h\Delta t$$\
This is an approximation of [Fourier's Law](https://en.wikipedia.org/wiki/Thermal_conduction#Fourier.27s_law:~:text=temperature%2C%20gives%20the-,heat%20flow%20rate%20as,-%F0%9D%91%84).
The calculation is similar, except that the simulation assumes the time step is small enough that heat from one cell would only non-negligibly reach its neighbors during the time step, and would go no further until the next time step.

## Current Implementation

I first attempted to implement the temperature system in pure C# (i.e. running it on the CPU), but this ended up being extremely slow. For a 10,000 cell grid, it took a few seconds to compute a single time step. For smaller grids of around 1000 cells, the framerate still dropped significantly. To keep my game immersive, I needed a much more performant solution.

My first thought to solve this was to use ECS and multithreading, but then my friend suggested using [compute shaders](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ComputeShader.html).
Since this simulation could be computed using massively parallel identical computations for each cell, running it on the GPU was a great solution, and much simpler to implement than ECS.

The compute shader implementation was quite simple thanks to Unity's API. To run the diffusion calculations, the compute shader [itself](https://github.com/nathanrun1/temperature-diffusion-sim/blob/main/Assets/Shaders/HeatDiffusion.compute) implements the algorithm described above. On the CPU, the actual temperature grid is stored and passed to the shader using [compute buffers](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ComputeBuffer.html).

On the CPU/RAM side, the entire temperature grid for the whole scene is stored as a flattened array. The script that interacts with the compute shader then stores a dedicated "window", which is a copy of a smaller specific region of the whole grid, meant to be the region currently surrounding the player.

The compute shader uses an "input" compute buffer and and "output" compute buffer of identical size. At each simulation step, the input buffer is filled with a copy of the data in the "window" region. Then, the compute shader computes the temperature change for each of the cells in the given input buffer, and sends the updated temperature values to the corresponding cells in the output buffer.

For visualization purposes, the compute shader also has a secondary kernel (essentially another function that is called by the CPU to run on the GPU) that takes the current input buffer and displays its temperature values onto a given texture. The color of each cell is [linearly interpolated](https://www.cuemath.com/linear-interpolation-formula/) from blue to red, where 0 degrees is blue and 20 degrees is red.

## Results

Although its going to be used for much cooler stuff in the actual game, this system produced some pretty interesting (and performant!) results. There was no noticeable framerate change on grids even up to and past 10,000 cells, even with the simulation step occuring at every frame.

Combined with insulation and heat sources, it can easily be seen what type of immersive interactions could be produced in the game.

E.g. imagine here the player places a campfire to warm their shelter up (green represents high insulation on a cell), and then some enemy knocks their walls down.

https://github.com/user-attachments/assets/9ed96e6f-b217-4c02-aafc-e811d0b0909f

## Try it out yourself

To try it out, you'll need to open the project in Unity. The project is in Unity v6000.0.43f1, you can install Unity Hub and get started [here](https://unity.com/unity-hub) if you're unfamiliar.

Clone the repository and add the project folder to Unity Hub, and then open the project. Once you're in the project, navigate to Assets/Scenes/Main in the "Project" window, and then open the "Main" scene. In the Hierarchy, under Environment there is an empty GameObject named "TemperatureTest". Click on it and open the inspector. Here, you can set all of the config values for the simulation. I recommend a 16x16 "Local Grid" (outlined in blue).

<img width="341" alt="Screenshot 2025-04-16 211434" src="https://github.com/user-attachments/assets/e8c8e691-377a-42b5-a55a-ec9791618376" />

If your grid is small enough, you should see that one of the squares is transparent. This is your currently selected square, and you can move it with the arrow keys. In the inspector, (with TemperatureTest selected in the Hierarchy), you can 
set the Temperature, Insulation and Source Heat values (outlined in red). These values will be assigned to your selected square when you press space, so that you can experiment with the simulation. You can also control the simulation speed in the inspector with the "Time Scale" slider.

If you want to simulate walls, for example, you can set squares with high insulation values (near or equal to 1). If you want to simulate a campfire or some other type of heat source, just set the source heat to some positive value. The source heat value can also be negative, which can lead to some interesting results with two opposite sources near eachother.
