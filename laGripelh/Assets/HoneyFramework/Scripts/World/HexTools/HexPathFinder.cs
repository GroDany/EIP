using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;

namespace HoneyFramework
{
    /*
     * Simple call to produce world graph. More functionality could be provided to cover or translate advantages of the A* into hex world
     */
    public class HexPathFinder
    {
        static public HexGraph GenerateWorldGraph(AstarPath pathfinder)
        {
            HexGraph graph = new HexGraph();
            graph.active = pathfinder;
            graph.ScanInternal(null);

            return graph;
        }

    }
}