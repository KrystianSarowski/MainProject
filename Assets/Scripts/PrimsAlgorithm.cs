using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrimsAlgorithm
{
    public static List<NodeArc> primMST(Room[] t_rooms, int t_numOfRooms)
    {
        foreach(Room room in t_rooms)
        {
            room.SetIsVisited(false);
        }

        List<NodeArc> arclist = new List<NodeArc>();
        List<NodeArc> treeArcs = new List<NodeArc>();

        foreach(NodeArc arc in t_rooms[0].m_nodeArcs)
        {
            arclist.Add(arc);
        }

        arclist = arclist.OrderBy(o => o.GetWeigtht()).ToList();

        t_rooms[0].SetIsVisited(true);

        NodeArc curArc;

        while (arclist.Count() != 0)
        {
            curArc = arclist[0];

            if(curArc.GetTargetRoom().GetIsVisited() != true)
            {
                treeArcs.Add(curArc);
                curArc.GetTargetRoom().SetIsVisited(true);

                foreach (NodeArc arc in curArc.GetTargetRoom().m_nodeArcs)
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
