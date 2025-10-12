namespace Lab3.ProgramLoop
{
    // Класс для хранения результатов расчета
    public class StabilizerResult
    {
        public int Id { get; set; }
        public double Nu1 { get; set; } // Степень равенства по критерию 1
        public double Nu2 { get; set; } // Степень равенства по критерию 2
        public double Nu { get; set; }  // Интегральная оценка
    }
}
