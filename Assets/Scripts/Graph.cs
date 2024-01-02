using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    public List<GraphNode> nodes;

    public void ValidateNeighbors()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes[i].neighbors.Count; j++)
            {
                if (nodes[i].neighbors[j] == null)
                {
                    continue;
                }
                nodes[i].neighbors[j].AddNeighbor(nodes[i]);
            }
        }
    }

    public bool IsAdjacent(GraphNode a, GraphNode b)
    {
        return a.IsAdjacent(b);
    }

    public void AddVertex(GraphNode a)
    {
        nodes.Add(a);
        foreach (GraphNode g in a.neighbors)
        {
            g.AddNeighbor(a);
        }
    }
    public void RemoveVertex(GraphNode b)
    {
        foreach (GraphNode g in b.neighbors)
        {
            g.RemoveNeighbor(b);
        }
        nodes.Remove(b);
    }

    public void AddEdge(GraphNode a, GraphNode b)
    {
        a.AddNeighbor(b);
        b.AddNeighbor(a);
    }
    public bool RemoveEdge(GraphNode a, GraphNode b)
    {
        bool c = a.RemoveNeighbor(b);
        c |= b.RemoveNeighbor(a);
        return c;
    }

    public bool InsertInEdge(GraphNode a, GraphNode b, GraphNode insert)
    {
        if (!a.IsAdjacent(b))
        {
            return false;
        }
        a.RemoveNeighbor(b);
        a.AddNeighbor(insert);
        b.RemoveNeighbor(a);
        b.AddNeighbor(insert);
        insert.AddNeighbor(a);
        insert.AddNeighbor(b);
        return true;
    }
    public bool ContractEdge(GraphNode a, GraphNode b)
    {
        if (!a.IsAdjacent(b))
        {
            return false;
        }

        for (int i = 0; i < b.neighbors.Count; i++)
        {
            a.AddNeighbor(b.neighbors[i]);
        }
        RemoveVertex(b);
        return true;
    }    //B is destroyed

    public class GraphNode
    {
        public T inner;
        public List<GraphNode> neighbors;

        public GraphNode(T inner)
        {
            this.inner = inner;
            neighbors = new List<GraphNode>();
        }
        public GraphNode(T inner, List<GraphNode> neighborList)
        {
            this.inner = inner;
            neighbors = neighborList;
        }

        public void AddNeighbor(GraphNode a)
        {
            if (IsAdjacent(a) || a == this)
            {
                return;
            }
            neighbors.Add(a);
        }
        public bool RemoveNeighbor(GraphNode a)
        {
            return neighbors.Remove(a);
        }

        public bool IsAdjacent(GraphNode g)
        {
            return neighbors.Contains(g);
        }
    }
    public class Edge
    {
        public GraphNode[] vertices = new GraphNode[2];

        public Edge(GraphNode a, GraphNode b)
        {
            vertices = new GraphNode[2];
            vertices[0] = a;
            vertices[1] = b;
        }

        public void InterchangeVertices()
        {
            GraphNode t = vertices[0];
            vertices[0] = vertices[1];
            vertices[1] = t;
        }

        public override bool Equals(object obj)
        {
            Edge e = (Edge)obj;
            if (e != null)
            {
                return (e.vertices[0].Equals(vertices[0]) && e.vertices[1].Equals(vertices[1])) || (e.vertices[0].Equals(vertices[1]) && e.vertices[1].Equals(vertices[0]));
            } else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            //vertex 0 and 1 are interchangable
            return 911371710 + EqualityComparer<GraphNode>.Default.GetHashCode(vertices[0]) + EqualityComparer<GraphNode>.Default.GetHashCode(vertices[1]);
        }
    }
}
