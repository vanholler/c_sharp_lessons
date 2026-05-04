namespace LessonOop
{
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit)
            : base($"Длина задачи '{taskLength}' больше допустимого значение {taskLengthLimit}")
        {
        }
    }
}
