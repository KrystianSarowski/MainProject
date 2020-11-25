using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrimsAlgorithm
{
    public static List<TileArc> primMST(Room[] t_rooms)
    {
        foreach(Room room in t_rooms)
        {
            room.SetIsVisited(false);
        }

        List<TileArc> arclist = new List<TileArc>();
        List<TileArc> treeArcs = new List<TileArc>();

        foreach(TileArc arc in t_rooms[0].m_nodeArcs)
        {
            arclist.Add(arc);
        }

        arclist = arclist.OrderBy(o => o.GetWeigtht()).ToList();

        t_rooms[0].SetIsVisited(true);

        TileArc curArc;

        while (arclist.Count() != 0)
        {
            curArc = arclist[0];

            if(curArc.GetTargetRoom().GetIsVisited() != true)
            {
                treeArcs.Add(curArc);
                curArc.GetTargetRoom().SetIsVisited(true);

                foreach (TileArc arc in curArc.GetTargetRoom().m_nodeArcs)
                {
                    arclist.Add(arc);
                }
                arclist = arclist.OrderBy(o => o.GetWeigtht()).ToList();
            }

            else
            {
                arclist.Remove(curArc);
            }
        }

        return treeArcs;
    }
}
