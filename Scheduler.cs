using System;
using System.Collections.Generic;

namespace SchedulerSystem
{
    /// <summary>
    /// Scheduler- Рассписание хранит две коллекции TimeConditions NoteCondition
    /// </summary>
    public class Scheduler
    {
        private int currentСheckPoint;
        private Dictionary<int, Condition> CheckPoints;
        private Dictionary<Condition, Action> ConditionHandlers;
        private Queue<Condition> WorkingConditions;

        /// <summary>
        /// По умолчанию Scheduler начинает СheckPoint c Нуля
        /// </summary>
        /// <param name="currentСheckPoint"></param>
        public Scheduler(int currentСheckPoint = 0)
        {
            this.currentСheckPoint = currentСheckPoint;
            ConditionHandlers = new Dictionary<Condition, Action>();
            CheckPoints = new Dictionary<int, Condition>();
            WorkingConditions = new Queue<Condition>();
        }

        ///<summary>
        /// Не добавит в Scheduler если там уже есть похожее имя, или сама кондиция уже ранее добавлена
        /// !!!Внимание!!! Если функция определит коллизию в рассписании то возможны смещения на соседние Пойнты
        ///</summary>
        /// <param name="newCondition"> Возможны изменения данных аргумента</param>
        public void Add(Condition newCondition, Action CallBack)
        {

            bool nameIsExist = new List<Condition>(ConditionHandlers.Keys).Exists((x) => x.Name.ToUpper() == newCondition.Name.ToUpper());

            if (newCondition.Name == "" || nameIsExist) // Имя пустое или уже существует в коллекции! Добавления не будет
            {
                throw new Exception($"Condition {newCondition.Name} не может добавиться в Scheduler! Имя не должно быть пустым, или уже такое существует");
            }

            if (newCondition.GetAllpriorityPoints().Count > 0)
            {
                AddCheckPoints(newCondition);
            }
            newCondition.START();
            ConditionHandlers[newCondition] = CallBack;

        }

        ///<summary>
        ///Если аргументов нету, то будут вызываться все кондиции добавленные в Scheduler.
        ///Определяет есть ли среди бэйджиков рабочая кондиция, если есть то
        ///вызывает callBack привязанный к данной кондиции и делает авто START().
        ///Если возвращает Null значит из бэйджиков пока не настало время, либо их вообще нету в Scheduler.
        ///!!!Внимание!!! Всегда в приоритете на вызов будут кондиции совпадающие с PriorityPoints с currentСheckPoint.
        ///</summary>
        public Condition Invoke(params Condition[] badges)
        {
            List<Condition> badgeList;
            Condition WorkingCondition = null;
            if (badges == null || badges.Length == 0)
            {
                badgeList = new List<Condition>(ConditionHandlers.Keys);
            }
            else
            {
                badgeList = new List<Condition>(badges);
            }

            //Сначала проверка Кондиции на Приоритетный Пойнт
            if (CheckPoints.ContainsKey(currentСheckPoint))
            {
                if (badgeList.Contains(CheckPoints[currentСheckPoint]))
                {
                    WorkingCondition = CheckPoints[currentСheckPoint];
                    ConditionHandlers[WorkingCondition]();
                    return WorkingCondition;
                }
            }

            // Кондиция в любой момент может изменить своё состояние поэтому здесь идет контроль на готовность, а также цикл на случай если будут выброшенны
            // те что стали неготовыми
            while (WorkingConditions.Count > 0)
            {
                if (WorkingConditions.Peek().IsReady())
                {
                    if (badgeList.Contains(WorkingConditions.Peek()))
                    {
                        WorkingCondition = WorkingConditions.Dequeue();
                        ConditionHandlers[WorkingCondition]();
                        WorkingCondition.START();
                        return WorkingCondition;
                    }
                }
                else
                {
                    WorkingConditions.Dequeue(); // Просто выбрасываются, они изменили свое состояние

                    if (WorkingConditions.Count == 0) // Попытка продлить цикл путем нахождения новых готовых Кондиций
                    {
                        UpdateQueue();
                    }
                    continue;
                }
                return null;
            }
            return WorkingCondition;
        }

        /// <summary>
        /// Возвращает добавленные кондиции из Scheduler по имени. Не чувствителен к регистру. Если аргументов нет то возращает все кондиции.
        /// </summary>
        /// <param name="conditionName"></param>
        /// <returns></returns>
        public List<Condition> GetConditions(params string[] conditionName)
        {
            List<Condition> wantedConditions;
            if (conditionName == null || conditionName.Length == 0)
            {
                wantedConditions = new List<Condition>(ConditionHandlers.Keys);
            }
            else
            {
                wantedConditions = new List<Condition>();
                foreach (string name in conditionName)
                {
                    foreach (Condition item in ConditionHandlers.Keys)
                    {
                        if (item.Name.ToUpper() == name.ToUpper())
                        {
                            wantedConditions.Add(item);
                        }
                    }
                }
            }
            return wantedConditions;
        }

        /// <summary>
        /// Не забывайте вызывать
        /// </summary>
        public void NextCheckPoint(int nextCheckPoint = 0)
        {
            if (nextCheckPoint > 0)
            {
                currentСheckPoint = nextCheckPoint;
            }
            else
            {
                currentСheckPoint++;
            }

            foreach (Condition item in ConditionHandlers.Keys)
            {
                item.NextSkip();
            }
            UpdateQueue();
        }

        public List<int> GetAllCheckPoints()
        {
            return new List<int>(CheckPoints.Keys);
        }

        /// <summary>
        /// Удаляет полностью и все следы кондиции из Scheduler
        /// </summary>
        /// <param name="condition"></param>
        public void Remove(Condition condition)
        {
            foreach (int key in CheckPoints.Keys)
            {
                if (CheckPoints[key] == condition)
                {
                    CheckPoints.Remove(key);
                }
            }
            ConditionHandlers.Remove(condition);

            while (WorkingConditions.Contains(condition))
            {
                Condition buffer = WorkingConditions.Dequeue();
                if (buffer != condition)
                {
                    WorkingConditions.Enqueue(buffer);
                }
            }

        }


        private bool AddCheckPoints(Condition newCondition)
        {
            bool ConditionAdded = false;
            List<int> AllNewPoints = newCondition.GetAllpriorityPoints();
            foreach (int point in AllNewPoints)
            {
                int newPoint = point;
                if (newPoint == 0) continue; // данная точка кондиции проверку не проходит, идем дальше
                while (CheckPoints.ContainsKey(newPoint)) // Переработка точки приоритета на случай если такая уже занята
                {
                    newPoint++;
                }
                CheckPoints[newPoint] = newCondition;
                ConditionAdded = true;
            }
            return ConditionAdded;
        }

        /// <summary>
        /// Добавляет в очередь те кондиции в которых истекло засеченное время И истекли установленные пропуски
        /// </summary>
        private void UpdateQueue()
        {
            foreach (Condition item in ConditionHandlers.Keys)
            {
                if (item.IsReady())
                {
                    WorkingConditions.Enqueue(item);
                }
            }
        }

    }

    /// <summary>
    /// Для активации таймера задайте поле setedSeconds.
    /// 
    /// Для активации счетчика скипов задайте поле setedSkips и вызывайте NextSkip() насколько это удобно.
    /// 
    /// Для запуска есть общий метод START(), есть ручные методы StartTimer() и ResetSkips().
    /// 
    /// После завершения Кондиции
    /// 
    /// Для контроля достигла ли Кондиция нуля есть метод IsReady(), есть ручные поля IsTimeUp и HasNextSkips.
    /// 
    /// Для точек обязательных для срабатывания задайте список priorityPoints но это не входит в IsReady() контролить нужно через 
    /// HasProrityPoint(int point)
    /// </summary>
    public class Condition
    {
        private string name;
        public int setedSeconds;
        public int setedSkips;
        private List<int> checkPoints;

        private DateTime startTimer;
        private int currentSkip;

        public bool IsTimeUp
        {
            get
            {
                double restSeconds = ((TimeSpan)(DateTime.Now - startTimer)).TotalSeconds;
                if (restSeconds > setedSeconds)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool HasNextSkips
        {
            get
            {
                return (currentSkip > 0) ? true : false;
            }
        }
        public string Name { get { return name; } }

        public Condition(string Name)
        {
            this.name = Name;
        }
        public Condition(string Name, List<int> checkPoints)
        {
            this.name = Name;
            this.checkPoints = checkPoints;
        }

        /// <summary>
        /// Главный метод контроля готовности Кондиции, можете использовать поля для личного контроля IsTimeUp и HasNextSkips.
        /// Также проверяет были ли вообще заведены какие либо счетчики
        /// </summary>
        /// <returns>Возвращает True если таймер истек, и нету больше скипов</returns>
        public bool IsReady()
        {
            bool timeIsSeted = (setedSeconds > 0) ? true : false;
            bool skipsIsSeted = (setedSkips > 0) ? true : false;

            bool isReady = false;

            if (timeIsSeted && skipsIsSeted)
            {
                isReady = IsTimeUp && !HasNextSkips;
            }
            else if (timeIsSeted)
            {
                isReady = IsTimeUp;
            }
            else if (skipsIsSeted)
            {
                isReady = !HasNextSkips;
            }

            return isReady;
        }
        /// <summary>
        /// Убавляет один Скип
        /// </summary>
        public void NextSkip()
        {
            if (currentSkip > 0)
            {
                currentSkip--;
            }
        }
        /// <summary>
        /// Сколько осталось скипов до готовности
        /// </summary>
        /// <returns></returns>
        public int GetRestSkips()
        {
            return currentSkip;
        }
        /// <summary>
        /// Без START() Кондиция всегда будет возращать true (IsReady()) .
        /// данный метод запускает StartTimer() и ResetSkips()
        /// </summary>
        public void START()
        {
            StartTimer();
            ResetSkips();
        }
        /// <summary>
        /// Ручной запуск таймера
        /// </summary>
        /// <param name="newSetSeconds"> Устанавливает на старте новый setSeconds</param>
        public void StartTimer(int newSetSeconds = 0)
        {
            if (newSetSeconds == 0)
            {
                startTimer = DateTime.Now;
            }
            else
            {
                setedSeconds = newSetSeconds;
                startTimer = DateTime.Now;
            }
        }
        /// <summary>
        /// Ручной запуск счетчика
        /// </summary>
        /// <param name="newSetSeconds"> Устанавливает на старте новый setedSkips</param>
        public void ResetSkips(int newSetSkips = 0)
        {
            if (newSetSkips == 0)
            {
                currentSkip = setedSkips;
            }
            else
            {
                setedSkips = newSetSkips;
                currentSkip = setedSkips;
            }
        }
        public int GetRestTime()
        {
            double restTime = ((TimeSpan)(startTimer - DateTime.Now)).TotalSeconds;
            return ((int)restTime + setedSeconds < 0) ? 0 : (int)restTime + setedSeconds;
        }
        public bool HasCheckPoint(int point)
        {
            return checkPoints.Contains(point);
        }
        public List<int> GetAllpriorityPoints()
        {
            List<int> newList = new List<int>();
            newList.AddRange(checkPoints);
            return newList;
        }

        /// <summary>
        /// Автоматический метод вызывает CallBack когда кондиция готова, и метод START() для её рестарта после вызова CallBack.
        /// Если priorityPoint не будет указан то Кондиция не будет учитывать список приоритетных точек
        /// </summary>
        /// <param name="CallBack"></param>
        /// <param name="priorityPoint"></param>
        public void AutoInvoke(Action CallBack, int checkPoint = 0)
        {
            if (checkPoint != 0 && checkPoints.Contains(checkPoint))
            {
                if (checkPoints.Contains(checkPoint))
                {
                    CallBack();
                    return;
                }
            }
            if (IsReady())
            {
                CallBack();
                StartTimer();
                ResetSkips();
            }
            else
            {
                NextSkip();
            }
        }
    }
}
