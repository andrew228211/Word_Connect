using System.Collections;
using System.Collections.Generic;
using System;

namespace GeneticAlgorithm
{
    public class BuildCrossWord
    {
        private const int POPULATION_SIZE = 100;
        private const double CROSSOVEER_RATE = 0.9f;
        private const double MUTATION_RATE = 1f;
        private const float RATIO = 0.1f;
        private const int RESTART_GENERATION = 100000;//thresold restart
        private Random random = new Random();
        private List<string> words = new List<string>();
        private int maxSize;
        public char[][] GetCrossWord(List<string> words,int maxSize)
        {
            this.words = words;
            this.maxSize = maxSize;

            List<GeneticCrossWord> population = InitPopulatioin();
            int generation = 0;
            int index = 0;
            long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (true)
            {
                // Nếu số lượng gereration vượt ngưỡng cho phép thì thuật toán cần phải restart lại
                if (generation >= RESTART_GENERATION)
                {
                    population = InitPopulatioin();
                    generation = 0;
                    index ++;
                }
                GeneticCrossWord bestCrossword=GetBestGenCrossword(population);
                GeneticCrossWord worstCrossword=GetWorstCrossword(population);
                float avgFitness=GetAverageFitness(population);
                if (bestCrossword.GetCurrentFitness() <= 0)
                {
                    long endTime= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    long elapsedTimeInSeconds= (endTime - startTime) / 1000;
                    string formattedTime=FormatTime(elapsedTimeInSeconds);
                    Console.WriteLine("Generation: " + generation + " | Average fitness: " + avgFitness + " | Worst fitness: " + worstCrossword.GetCurrentFitness() + " | Best fitness: " + bestCrossword.GetCurrentFitness());
                    Console.WriteLine("Time elapsed: "+ formattedTime);
                    char[][] grid = bestCrossword.GetGridForCrossWord();
                    return grid;
                }
                population= NewGeneration(population);
                generation++;
            }
        }
        /// <summary>
        /// Khởi tạo population cho danh sách từ hiện tại
        /// </summary>
        /// <returns>Trả về 1 danh sách population</returns>
        private List<GeneticCrossWord> InitPopulatioin()
        {
            List<GeneticCrossWord> population = new List<GeneticCrossWord>();
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                population.Add(new GeneticCrossWord(words, maxSize));
            }
            return population;
        }
        /// <summary>
        /// Tính giá trị trung biunfh fitness cho population tham gia
        /// </summary>
        /// <param name="population">population hiện tại</param>
        /// <returns></returns>
        private float GetAverageFitness(List<GeneticCrossWord> population) {
            int totalFitness = 0;
            foreach(GeneticCrossWord geneticCrossWord in population) {
                totalFitness += geneticCrossWord.GetCurrentFitness();
            }
            return (float)totalFitness / population.Count;
        }
        /// <summary>
        /// Lấy crossword dựa vào giá trị fitness nhỏ nhất
        /// </summary>
        /// <param name="population">population hiện tại</param>
        /// <returns></returns>
        private GeneticCrossWord GetBestGenCrossword(List<GeneticCrossWord> population) {
            GeneticCrossWord bestCrossword = population[0];
            foreach (GeneticCrossWord crossWord in population) {
                if (crossWord.GetCurrentFitness() < bestCrossword.GetCurrentFitness())
                {
                    bestCrossword = crossWord;
                }
            }
            return bestCrossword;
        }

        /// <summary>
        /// Lấy crossword dựa vào giá trị fitness lớn nhát
        /// </summary>
        /// <param name="population">population hiện tại</param>
        /// <returns></returns>
        private GeneticCrossWord GetWorstCrossword(List<GeneticCrossWord> population) {
            GeneticCrossWord worstCrossword = population[0];
            foreach (GeneticCrossWord crossWord in population)
            {
                if (crossWord.GetCurrentFitness() > worstCrossword.GetCurrentFitness())
                {
                    worstCrossword = crossWord;
                }
            }
            return worstCrossword;
        }
        private List<GeneticCrossWord> NewGeneration(List<GeneticCrossWord> population)
        {
            List<GeneticCrossWord> newGeneration = new List<GeneticCrossWord>();
            newGeneration.Add(new GeneticCrossWord(words, maxSize));
            for (int i = 1; i < POPULATION_SIZE; i++)
            {
                GeneticCrossWord parent1 = SelectParent(population);
                GeneticCrossWord parent2 = SelectParent(population);
                GeneticCrossWord child = Crossover(parent1, parent2);
                Mutate(child);
                newGeneration.Add(child);
            }
            return newGeneration;
        }
        //Cá nhân tốt nhất trong mẫu có quyền làm cha mẹ(parent)
        private GeneticCrossWord SelectParent(List<GeneticCrossWord> population)
        {
            int size = (int)(population.Count * RATIO);
            //Chọn random
            GeneticCrossWord bestCrossword = population[random.Next(population.Count)];
            for (int i = 1; i < size; i++)
            {
                GeneticCrossWord candidate = population[random.Next(population.Count)];
                if (candidate.GetCurrentFitness() < bestCrossword.GetCurrentFitness())
                {
                    bestCrossword = candidate;
                }
            }
            return bestCrossword;
        }
        private GeneticCrossWord Crossover(GeneticCrossWord parent1, GeneticCrossWord parent2) {
            if (random.NextDouble() < CROSSOVEER_RATE)
            {
                return parent1.CrossOver(parent2);
            }
            else
            {
                return parent1.Copy();
            }
        }
        private void Mutate(GeneticCrossWord geneticCrossWord) {
            if (random.NextDouble() < MUTATION_RATE) { 
                geneticCrossWord.Mutate();
            }
        }
        private string FormatTime(long seconds)
        {
            long hours = seconds / 3600;
            long remainder = seconds % 3600;
            long minutes = remainder / 60;
            long secs = remainder % 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, secs);
        }
    }
}
