namespace blazorSBIFS.Server.Tools
{
    public static class GraphTools
    {
        public static void SimplifyGraph(ref double[,] graph)
        {
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                for (int j = 0; j < graph.GetLength(1); j++)
                {
                    if (i == j) continue;
                    double val1 = graph[i, j];
                    double val2 = graph[j, i];

                    if (val1 > val2)
                    {
                        graph[i, j] = Math.Round((val1 - val2), 2, MidpointRounding.AwayFromZero);
                        graph[j, i] = 0.0;
                    }
                    else if (val2 > val1)
                    {
                        graph[j, i] = Math.Round((val2 - val1), 2, MidpointRounding.AwayFromZero);
                        graph[i, j] = 0.0;
                    }
                    else
                    {
                        graph[i, j] = 0.0;
                        graph[j, i] = 0.0;
                    }
                }
            }
        }
    }
}
