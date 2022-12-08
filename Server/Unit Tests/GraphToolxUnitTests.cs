using Xunit;

namespace blazorSBIFS.Server.Unit_Tests;
using blazorSBIFS.Server.Tools;

public class GraphToolxUnitTests
{
    [Fact]
    public void SimplifyGraphRemovesEdges() //This unit test passes.
    {
        // Create a small graph with some edges
        double[,] graph = new double[3, 3];
        graph = new double[,] {
            { 0, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 }
        };

        // Simplify the graph
        GraphTools.SimplifyGraph(ref graph);

        // Count the number of non-zero elements in the resulting matrix
        int nonZeroCount = 0;
        for (int i = 0; i < graph.GetLength(0); i++)
        {
            for (int j = 0; j < graph.GetLength(1); j++)
            {
                if (graph[i, j] != 0)
                {
                    nonZeroCount++;
                }
            }
        }

        // Verify that the number of non-zero elements is less than the original matrix
        Assert.True(nonZeroCount < 3);
    }

    [Fact]
    public void SimplifyGraphWithASingleVertex() //This unit test passes.
    {
        // Create a graph with a single vertex
        double[,] graph = new double[1, 1];
        graph = new double[,] {
            { 0 }
        };

        // Simplify the graph
        GraphTools.SimplifyGraph(ref graph);

        // Verify that the resulting matrix is the same as the original matrix
        Assert.Equal(graph, new double[,] {
            { 0 }
        });
    }

    [Fact]
    public void SimplifyGraphIsNotFullyConnected() //This unit test passes.
    {
        // Create a small graph with some vertices that are not connected to any other vertices
        double[,] graph = new double[3, 3];
        graph = new double[,] {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        // Simplify the graph
        GraphTools.SimplifyGraph(ref graph);

        // Verify that the resulting matrix is the same as the original matrix
        Assert.Equal(graph, new double[,] {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        });
    }

    [Fact]
    public void SimplifyGraphRemoveEdgesThatShouldNotBeRemoved() //This unit test it is supposed to fail succesfully.
    {
        // Create a small graph with some edges
        double[,] graph = new double[3, 3];
        graph = new double[,] {
            { 0, 1, 2 },
            { 1, 0, 3 },
            { 2, 3, 0 }
        };

        // Simplify the graph
        GraphTools.SimplifyGraph(ref graph);

        // Verify that the resulting matrix is the same as the original matrix
        Assert.Equal(graph, new double[,] {
            { 0, 1, 2 },
            { 1, 0, 3 },
            { 2, 3, 0 }
        });
    }
    [Fact]
    public void SimplifyGraphWithEdgesOfVaryingWeights() //This unit test it is supposed to fail.
    {
        // Create a small graph with some edges of varying weights
        double[,] graph = new double[3, 3];
        graph = new double[,] {
            { 0, 1, 2 },
            { 3, 0, 4 },
            { 5, 6, 0 }
        };

        // Simplify the graph
        GraphTools.SimplifyGraph(ref graph);

        // Verify that the resulting matrix is as expected
        Assert.Equal(graph, new double[,] {
            { 0, 1, 0 },
            { 3, 0, 4 },
            { 0, 6, 0 }
        });
    }

    [Fact]
    public void SimplifyGraphWithDisconnectedComponents() //This unit test it is supposed to fail.
    {
        // Create a graph with multiple disconnected components
        double[,] graph = new double[4, 4];
        graph = new double[,] {
            { 0, 1, 0, 0 },
            { 1, 0, 0, 0 },
            { 0, 0, 0, 2 },
            { 0, 0, 2, 0 }
        };

        // Simplify the graph
        GraphTools.SimplifyGraph(ref graph);

        // Verify that the resulting matrix is as expected
        Assert.Equal(graph, new double[,] {
            { 0, 1, 0, 0 },
            { 1, 0, 0, 0 },
            { 0, 0, 0, 2 },
            { 0, 0, 2, 0 }
        });
    }
}