namespace Lab3.ProgramLoop
{
    internal class Logic : IProgram
    {
        // Функция принадлежности для варианта 8
        static double MembershipFunction(double q, double delta, double x)
        {
            double numerator = delta * delta;
            double denominator = 255 * Math.Pow(x - q, 2) + delta * delta;
            return Math.Pow(numerator / denominator, 1.0 / 8.0);
        }

        // Расчет степени равенства двух нечетких множеств
        static double CalculateNu(double q_ij, double delta_ij, double q_0j, double delta_0j)
        {
            // Точка пересечения функций принадлежности
            double x_star = (q_ij * delta_0j + q_0j * delta_ij) / (delta_0j + delta_ij);

            // Степень равенства = значение функции принадлежности эталона в точке пересечения
            return MembershipFunction(q_0j, delta_0j, x_star);
        }

        //Получить значения альфа от пользователя
        static double[] GetAlpha()
        {
            double[] alpha = new double[2];
            Console.WriteLine("Введите веса критериев (сумма должна быть равна 1):");

            Console.Write("Вес критерия 1 (температура плавления): ");
            alpha[0] = double.Parse(Console.ReadLine());

            Console.Write("Вес критерия 2 (концентрация в ПЭ): ");
            alpha[1] = double.Parse(Console.ReadLine());

            //Проверка альфа на корректность
            if (alpha[0] < 0 || alpha[1] < 0 || alpha[0] + alpha[1] != 1)
            {
                throw new WrongAlphaValueException();
            }

            return alpha;
        }
        public Logic()
        {
            StartProgram();
        }

        public void StartProgram()
        {
            var stabilizers = new Dictionary<int, double[]>
            {
                { 8, new double[] {147, 1, 2.6, 2.4}},      // центр [0.2;5] = 2.6, радиус = 2.4
                { 9, new double[] {49, 1, 1.0, 0.5}},       // центр [0.5;1.5] = 1.0, радиус = 0.5
                { 10, new double[] {131.8, 1, 1.25, 0.75}}, // центр [0.5;2] = 1.25, радиус = 0.75
                { 11, new double[] {54, 1, 0.3, 0.2}},      // центр [0.1;0.5] = 0.3, радиус = 0.2
                { 12, new double[] {-5, 1, 0.2, 0.1}},      // центр [0.1;0.3] = 0.2, радиус = 0.1
                { 13, new double[] {218, 1, 0.105, 0.095}}  // ЭТАЛОН: центр [0.01;0.2] = 0.105, радиус = 0.095
            };

            double[] alpha = GetAlpha();

            // Эталонные значения (стабилизатор №13)
            double[] reference = stabilizers[13];
            double q01 = reference[0], delta01 = reference[1];
            double q02 = reference[2], delta02 = reference[3];

            Console.WriteLine($"\nЭталон (стабилизатор 13):");
            Console.WriteLine($"Температура плавления: q01 = {q01}, b01 = {delta01}");
            Console.WriteLine($"Концентрация в ПЭ: q02 = {q02}, b02 = {delta02}");
            Console.WriteLine($"Веса критериев: a1 = {alpha[0]}, a2 = {alpha[1]}");
            Console.WriteLine("\n" + new string('-', 60));

            // Расчет интегральных оценок для всех стабилизаторов и вывод информации
            CalculateResult(q01, delta01, q02, delta02, stabilizers, alpha);
        }

        public void CalculateResult(double q01, double delta01, double q02, double delta02, Dictionary<int, double[]> stabilizers, double[] alpha)
        {
            var results = new Dictionary<int, StabilizerResult>();


            foreach (var stabilizer in stabilizers)
            {
                int id = stabilizer.Key;
                if (id == 13) continue;

                double[] params_ij = stabilizer.Value;
                double q_i1 = params_ij[0], delta_i1 = params_ij[1];
                double q_i2 = params_ij[2], delta_i2 = params_ij[3];

                // Расчет степеней равенства по каждому критерию
                double nu_i1 = CalculateNu(q_i1, delta_i1, q01, delta01);
                double nu_i2 = CalculateNu(q_i2, delta_i2, q02, delta02);

                // Интегральная оценка
                double nu_i = alpha[0] * nu_i1 + alpha[1] * nu_i2;

                results[id] = new StabilizerResult
                {
                    Id = id,
                    Nu1 = nu_i1,
                    Nu2 = nu_i2,
                    Nu = nu_i
                };
            }

            // Упорядочивание по убыванию интегральной оценки
            var rankedStabilizers = results.Values.OrderByDescending(r => r.Nu).ToList();

            // Вывод результатов
            Console.WriteLine("\nРезультаты ранжирования светостабилизаторов ПЭ:");
            Console.WriteLine("№\tv_1\tv_2\tv\tМесто");
            Console.WriteLine(new string('-', 50));

            for (int i = 0; i < rankedStabilizers.Count; i++)
            {
                var result = rankedStabilizers[i];
                Console.WriteLine($"{result.Id}\t{result.Nu1:F4}\t{result.Nu2:F4}\t{result.Nu:F4}\t{i + 1}");
            }

            // Детальная информация по каждому стабилизатору
            Console.WriteLine("\nДетальная информация:");
            Console.WriteLine(new string('-', 50));

            foreach (var result in rankedStabilizers)
            {
                double[] params_ij = stabilizers[result.Id];
                Console.WriteLine($"\nСтабилизатор {result.Id}:");
                Console.WriteLine($"Температура: q = {params_ij[0]}, b = {params_ij[1]}");
                Console.WriteLine($"Концентрация: q = {params_ij[2]}, b = {params_ij[3]}");
                Console.WriteLine($"Степени равенства: v_1 = {result.Nu1:F4}, v_2 = {result.Nu2:F4}");
                Console.WriteLine($"Интегральная оценка: v = {result.Nu:F4}");
            }

            Console.WriteLine($"\nЛучший светостабилизатор: №{rankedStabilizers[0].Id} с v = {rankedStabilizers[0].Nu:F4}");
        }
    }
}