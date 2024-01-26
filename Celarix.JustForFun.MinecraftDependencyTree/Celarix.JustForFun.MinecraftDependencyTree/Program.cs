namespace Celarix.JustForFun.MinecraftDependencyTree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        // For instance:
        // 1 Golden Pickaxe requires:
        // - 3 Gold Ingots
        //  - 3 Raw Gold
        //      - 1.36 Gold Ore (with Fortune III)
        //          - 1 Iron Pickaxe (with Fortune III)
        //              - 3 Iron Ingots
        //              - 2 Sticks
        //  - 3/100ths of a Block of Coal
        // - 2 Sticks
        // and so forth
    }
}