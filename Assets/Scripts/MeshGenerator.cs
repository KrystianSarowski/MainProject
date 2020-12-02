using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid m_squareGrid;
    List<Vector3> m_vertices;
    List<int> m_triangles;

    public void GenerateMesh(TileGrid t_tileGrid, float t_squareSize)
    {
        m_squareGrid = new SquareGrid(t_tileGrid, t_squareSize);

        m_vertices = new List<Vector3>();
        m_triangles = new List<int>();

        for (int x = 0; x < m_squareGrid.m_squares.GetLength(0); x++)
        {
            for (int y = 0; y < m_squareGrid.m_squares.GetLength(1); y++)
            {
                TriangulateSquare(m_squareGrid.m_squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square)
    {
        switch (square.m_configuration)
        {
            case 0:
                break;

            //1 corner is active.
            case 1:
                MeshFromPoints(square.m_centreBottom, square.m_bottomLeft, square.m_centreLeft, square.m_centre);
                break;
            case 2:
                MeshFromPoints(square.m_centreRight, square.m_bottomRight, square.m_centreBottom, square.m_centre);
                break;
            case 4:
                MeshFromPoints(square.m_centreTop, square.m_topRight, square.m_centreRight, square.m_centre);
                break;
            case 8:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centre, square.m_centreLeft);
                break;

            //2 corner are active.
            case 3:
                MeshFromPoints(square.m_centreRight, square.m_bottomRight, square.m_bottomLeft, square.m_centreLeft);
                break;
            case 6:
                MeshFromPoints(square.m_centreTop, square.m_topRight, square.m_bottomRight, square.m_centreBottom);
                break;
            case 9:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centreBottom, square.m_bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.m_topLeft, square.m_topRight, square.m_centreRight, square.m_centreLeft);
                break;
            case 5:
                MeshFromPoints(square.m_centreTop, square.m_topRight, square.m_centreRight, square.m_centreBottom, square.m_bottomLeft, square.m_centreLeft);
                break;
            case 10:
                MeshFromPoints(square.m_topLeft, square.m_centreTop, square.m_centreRight, square.m_bottomRight, square.m_centreBottom, square.m_centreLeft);
                break;

            //3 corners are active this is a unique case for which the triangles need to be done in special way.
            case 7:
                AssignVertices(square.m_centreTop, square.m_topRight, square.m_bottomRight, square.m_centreBottom, square.m_bottomLeft, square.m_centreLeft, square.m_centre);
                CreateTriangle(square.m_centreTop, square.m_topRight, square.m_bottomRight);
                CreateTriangle(square.m_centreTop, square.m_bottomRight, square.m_centreBottom);
                CreateTriangle(square.m_centreBottom, square.m_bottomLeft, square.m_centreLeft);
                CreateTriangle(square.m_centreBottom, square.m_centreLeft, square.m_centre);
                break;
            case 11:
                AssignVertices(square.m_topLeft, square.m_centreTop, square.m_centre, square.m_centreRight, square.m_bottomRight, square.m_centreBottom, square.m_bottomLeft);
                CreateTriangle(square.m_topLeft, square.m_centreTop, square.m_centreBottom);
                CreateTriangle(square.m_topLeft, square.m_centreBottom, square.m_bottomLeft);
                CreateTriangle(square.m_centre, square.m_centreRight, square.m_bottomRight);
                CreateTriangle(square.m_centre, square.m_bottomRight, square.m_centreBottom);
                break;
            case 13:
                AssignVertices(square.m_topLeft, square.m_topRight, square.m_centreRight, square.m_centre, square.m_centreBottom, square.m_bottomLeft, square.m_centreLeft);
                CreateTriangle(square.m_topLeft, square.m_topRight, square.m_centreRight);
                CreateTriangle(square.m_topLeft, square.m_centreRight, square.m_centreLeft);
                CreateTriangle(square.m_centre, square.m_centreBottom, square.m_bottomLeft);
                CreateTriangle(square.m_centre, square.m_bottomLeft, square.m_centreLeft);
                break;
            case 14:
                AssignVertices(square.m_topLeft, square.m_topRight, square.m_centreRight, square.m_bottomRight, square.m_centreBottom, square.m_centre, square.m_centreLeft);
                CreateTriangle(square.m_topLeft, square.m_topRight, square.m_centreRight);
                CreateTriangle(square.m_topLeft, square.m_centreRight, square.m_centreLeft);
                CreateTriangle(square.m_centreRight, square.m_bottomRight, square.m_centreBottom);
                CreateTriangle(square.m_centreRight, square.m_centreBottom, square.m_centre);
                break;

            //4 corners are active.
            case 15:
                MeshFromPoints(square.m_topLeft, square.m_topRight, square.m_bottomRight, square.m_bottomLeft);
                break;
        }
    }

    void MeshFromPoints(params Vertex[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }

        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }

        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }

        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(params Vertex[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].m_id == -1)
            {
                points[i].m_id = m_vertices.Count;
                m_vertices.Add(points[i].m_position);
            }
        }
    }

    void CreateTriangle(Vertex a, Vertex b, Vertex c)
    {
        m_triangles.Add(a.m_id);
        m_triangles.Add(b.m_id);
        m_triangles.Add(c.m_id);
    }

    public class SquareGrid
    {
        public Square[,] m_squares;

        public SquareGrid(TileGrid t_tileGrid, float t_squareSize)
        {
            int mapWidth = t_tileGrid.m_width;
            int mapHight = t_tileGrid.m_height;

            PrimaryVertex[,] primaryVertexGrid = new PrimaryVertex[mapWidth, mapHight];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHight; y++)
                {
                    Vector3 position = new Vector3(x * t_squareSize + t_squareSize / 2, 0, y * t_squareSize + t_squareSize / 2);
                    primaryVertexGrid[x, y] = new PrimaryVertex(position, t_tileGrid.GetTile(x, y).GetTileType() == TileType.Wall, t_squareSize);

                }
            }

            m_squares = new Square[mapWidth - 1, mapHight - 1];

            for (int x = 0; x < mapWidth - 1; x++)
            {
                for (int y = 0; y < mapHight - 1; y++)
                {
                    m_squares[x, y] = new Square(primaryVertexGrid[x, y + 1], primaryVertexGrid[x + 1, y + 1], primaryVertexGrid[x + 1, y], primaryVertexGrid[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public PrimaryVertex m_topLeft, m_topRight, m_bottomLeft, m_bottomRight;
        public Vertex m_centreLeft, m_centreTop, m_centreRight, m_centreBottom, m_centre;
        public int m_configuration;

        public Square(PrimaryVertex t_topLeft, PrimaryVertex t_topRight, PrimaryVertex t_bottomRight, PrimaryVertex t_bottomLeft)
        {
            m_topLeft = t_topLeft;
            m_topRight = t_topRight;
            m_bottomRight = t_bottomRight;
            m_bottomLeft = t_bottomLeft;

            m_centreTop = m_topLeft.m_right;
            m_centreRight = m_bottomRight.m_top;
            m_centreBottom = m_bottomLeft.m_right;
            m_centreLeft = m_bottomLeft.m_top;
            m_centre = m_bottomLeft.m_topRight;

            if (m_topLeft.m_isActive)
            {
                m_configuration += 8;
            }
                
            if (m_topRight.m_isActive)
            {
                m_configuration += 4;
            }
                
            if (m_bottomRight.m_isActive)
            {
                m_configuration += 2;
            }
                
            if (m_bottomLeft.m_isActive)
            {
                m_configuration += 1;
            }
        }
    }

    public class Vertex
    {
        public Vector3 m_position;
        public int m_id = -1;

        public Vertex(Vector3 t_position)
        {
            m_position = t_position;
        }
    }

    public class PrimaryVertex : Vertex
    {
        public bool m_isActive;
        public Vertex m_top, m_right, m_topRight;

        public PrimaryVertex(Vector3 t_pos, bool t_isActive, float t_distToNextPrimary) : base(t_pos)
        {
            m_isActive = t_isActive;

            Vector3 vertexPos = m_position + Vector3.forward * t_distToNextPrimary / 2.0f;
            m_top = new Vertex(vertexPos);

            vertexPos = m_position + Vector3.right * t_distToNextPrimary / 2.0f;
            m_right = new Vertex(vertexPos);

            vertexPos = m_position + Vector3.right * t_distToNextPrimary / 2.0f + Vector3.forward * t_distToNextPrimary / 2.0f;
            m_topRight = new Vertex(vertexPos);
        }
    }
}

