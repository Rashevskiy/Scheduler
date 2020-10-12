# Sheduler
ShedulerSystem.UnityPackage имеет демо сцену где вы можете поиграться в классе ManagerUI с Sheduler
<h1>Sheduler удобный расспорядок вызова функций, моя система кондиций</h1>
    <p>    Изначально инструмент был написан для помощи в разработке простых казуальных игр на Unity, по крайней мере, там он был весьма полезен. 
    Но я, думаю, прибегну к нему и в других проектах, и не только на юнити.
    И, возможно, он пригодится и вам!
    </p> 
<hr>
<h4>И так, для чего он?</h4>
     Представим ситуацию: вы написали несложную казуалку, в которой нужно тапнуть по экрану десять раз, чтобы пройти один уровень, и отправили ее на превью начальнику (или избалованному заказчику). 
Следом одна за одной приходят задачи:
<ol>
<li> А давай добавим рекламы на финише каждые 1,5 минуты?</li>
<li> А давай после нее где-нибудь на старте будет окно: "Купите премиум чтобы не было рекламы"?</li>
<li> Че-то это окно резко давит, а давай оно будет после рекламы, но только когда игрок нажал старт и прошел два уровня? </li>
<li> А давай на 10, 20, 30 уровне будет окно "поделитесь с друзьями"?</li>
<li> А давай с 10 уровня каждые 2 левела будет окно "Оцените нас!"?</li>
<li> И кучу еще всего!</li>
</ol>
<p>
    Даже пройдя через жестокий расстрел и бесконечный перекрой своего чада, проведя сотую проверку расположения окон, вы, рано или поздно, столкнетесь с такой проблемой: окна, привязанные к таймеру могут накладываться поверх окон, привязанных к самим уровням! Переделать это становится все более затруднительно - ведь проверок условий и так по горло, плюс ваш коллега мог добавить свои окна с совершенно нечитабельным кодом! Что же делать?
</p>
<hr>
<h4>Задача:</h4>
        Вызывать окно в определенный момент (например, нажатие на кнопку), и только в том случае, если выполнены все заданные условия (время прошло, достигли необходимого пойнта и т.д.)
<hr>
<h4>Решение:</h4>
        Для этого я сделал класс Condition, вот основные его поля:
<ol>
   <li> таймер <em>int setedSeconds</em></li> 
   <li> скипы <em>int setedSkips</em></li> 
   <li> список чекПойнтов <em>List &ltint&gt checkPoints</em></li>
</ol>
<p>
    Пока что, для общего понимания, начнем с мануальной настройки Кондиций. Потом и вовсе будет возможно задействовать сложный алгоритм вызова в одной строке.
    <br>
Таймер и скипы работают вместе, почти одинаково, оба имеют обратный отсчет. Разве что, для скипов необходимо придумать, на каком событии они будут убавляться, метод <em>NextSkip()</em>. Если значение таймера = 0, то он не активен, тоже самое и 
с скипами. Но даже если вы установили какое-нибудь значение, все равно нужно обязательно вызвать <em>START()</em> - он запустит их отсчет. Вы можете сделать
это индивидуально, в конкретных для себя случаях методы <em>StartTimer() и ResetSkips()</em>. Также, общий метод <em>IsReady()</em> 
возвращает true только когда те поля, что были активны (value > 0), после вызова метода <em>START()</em> достигли своего пика. 
</p>
<br>
<p>
В общем все просто: нужен таймер - ставите время (в секундах) таймеру <em>setedSeconds</em>, скипы оставляете на нуле, или наоборот,
или все вместе! <em>IsReady()</em> во всем разберется, главное всегда вызывать общий метод <em>START()</em> в том месте, где  
вы хотите, чтобы начался отсчет скипов.
</p>
<br>
<source lang="cs">
    
    public Condition myCondition;

    void Start(){
        myCondition = new Condition("Имя");
        myCondition.setedSeconds = 120; // 2 минуты
        myCondition.setedSkips = 5;
        myCondition.START(); // Отсчет пошел с самого старта игры
    }

    // Финиш какого нибудь уровня
    public void FinishRound(){
        myCondition.NextSkip(); // Левел пройден, скип убавился

        if (myCondition.IsReady())
        {
            // Мы достигли нужной кондиции, теперь можно выполнять какую нибудь функцию...

            myCondition.START(); //На этот раз START() как сброс на исходное. Обязательно, иначе каждый финиш будет IsReady == true
        }
    }
</source>
<br>
<p>
    Кондиция, как было написано выше, может хранить в себе отдельное условие - список чек пойнтов <em>List &ltint&gt checkPoints</em>. Этот список работает немного по-другому
    по той причине, что он, как правило приоритетный, т.е, нужен, если вы хотите какой-либо явный вызов функции строго по чекПойнтам. За чекПойнты можно принимать все, что угодно: пройденные лвлы, нажатия на кнопку или любые другие события (в основном, это уровни). Список чекПойнтов можно добавить исключительно через конструктор, он прилично инкапсулирован в целях дальнейшего удобного добавления в Sheduler класс, да и вряд ли будут причины по ходу программы менять список, ведь его идея - в ясности момента вызова.

</p>
<source lang="cs">






    public Condition myCondition;

    void Start(){
        myCondition = new Condition("имя",new List<int> { 1, 2, 5 }); // добавляем после обязательного имени, наш список лвлов
        myCondition.setedSeconds = 120;
        myCondition.setedSkips = 5;
        myCondition.START();
    }

    public void FinishRound(){
        myCondition.NextSkip(); 

        if (myCondition.IsReady() || myCondition.HasCheckPoint(currentLevel)) // добавили в условие имеет ли кондиция в списке текущий Левел
        {
            // Мы достигли нужной кондиции, теперь можно выполнять какую нибудь функцию...

            myCondition.START();
        }
    }
</source>
<p>
 Ну, если механика становится ясна, можно переходить на коробку автомат вообще без проблем =) У кондиции есть 
 <em>AutoInvoke(Action CallBack, int checkPoint = 0)</em> который освобождает от таких вызовов, как <em>NextSkip()</em> или повторного
 <em>START()</em> для перезапуска условий, но первый <em>START()</em> все равно требуется.
</p>


    public Condition myCondition;

    void Start(){
        myCondition = new Condition("имя",new List<int> { 1, 2, 5 });
        myCondition.setedSeconds = 120;
        myCondition.setedSkips = 5;
        myCondition.START(); 
    }

    public void FinishRound(){

        myCondition.AutoInvoke(() => Debug.Log("hello World"), currentLevel); 
        // можно без currentLevel, но тогда список чекПойнтов не будет учитываться                                                                         
    }
}

<br>
  <br><b> Один объект кондиции поможет вам быстро предоставить набор необходимых условий для срабатывания какой-либо одной функции!!! 
    Если у вас в ТЗ задача чуть сложнее, чем простой вызов, например, вызов через раз или через какое-то время, уже будет полезно прибегнуть к кондиции, ибо это абстракция и читабельность - вам лишь остается ее запустить и проверять ее готовность.</b> 
    <br>Я добавил в поля кондиции полезные юнити аттрибуты для удобной инициализации через 
    инспектор! 
<br><img src="https://habrastorage.org/webt/bx/rt/ir/bxrtirgnugccnd9ovpmggrrexjo.png" /> <br>
Главное - это гибкость, и если вдруг кто-то придумает добавить свой функционал, который не должен конфликтовать с вашей кондицией, вам просто нужно создать общий планировщик...
<hr>
<h4>next Задача:</h4>
Гибко добавлять разные окна (вызов функции) с разных мест программы, сохранять синхронизацию и избегать конфликтов наложения!
<hr>
Решение:
И вот мы добрались до главного класса <em>Sheduler</em> наш планировщик кондиций!
Объект данного класса лучше инициализировать как можно раньше. Конкретно в юнити лучше, чтобы объект был <em>DontDestroyOnLoad</em>.
Если заглянуть внутрь <em>Sheduler</em>, можно увидеть там такие поля:
<ol>
    <li>Текущий чек пойнт <em>int currentСheckPoint</em></li>
    <li>Коллекция всех добавленных кондиций и их поведения <em>Dictionary &ltCondition,Action&gt ConditionHandlers</em> - чтобы 
        планировщик знал, какую роль должна выполнить готовая кондиция</li>
    <li>Список чек пойнтов <em>Dictionary &ltint,Condition&gt CheckPoints</em>  - когда кондиция добавляется в Sheduler, её список добавляется в этот Dictionary, обязательно проверяя, свободен ли данный ключ. В противном случае он занимает след свободный чек пойнт.</li>
    <li>Очередь всех готовых кондиций <em>Queue &ltCondition&gt WaitingConditions</em> Если несколько кондиций становятся готовыми одновременно, 
        они откладываются на следующий вызов</li>
</ol>
Sheduler хранит поведение каждой кондиции и срабатывает согласно этому классу, задается он в момент добавления кондиции
<em>public void Add(Condition newCondition, Action CallBack)</em>,
где в аргументах есть обязательный делегат. Сам метод считывает имя кондиции и выбрасывает исключение, если оно пустое или уже добавлено - это нужно на случай, если по какой-то причине вам надо взять кондицию из расписания по имени <em>List&ltCondition&gt GetConditions(params string[] conditionName)</em>. Также, метод добавления <em>Add()</em> Сразу запускает <em>Start()</em>
добавленной кондиции. Это полезно, если запустить <em>Start()</em>
добавленной кондиции забудет кто из разработчиков, а также для того, чтобы избежать постоянного выбрасывания этой функции от Sheduler. Если вам нужно другое место для старта кондиции, вы просто работаете с кондицией как раньше, вы всегда можете менять её счетчики. В этом вся прелесть Sheduler - он обрабатывает, где кондиция готова, и где она изменила своей готовности, и делает этот расчет в момент вызова своего главного метода <em>Condition Invoke(params Condition[] badges)</em>. В аргументах вы можете указать некие бэйджики, т.е те кондиции, исключительно которые должны сработать, и те, чья очередь подошла, однако они не появились в списке бейджиков, то они не сработают. Но, если ничего не указывать, то, как и положено, каждый имеет право на вызов на пике очереди!

<b>Обязательно придумайте, где будет отсчет чек пойнтов для Sheduler <em>NextCheckPoint()</em>, например, на методе, на финише 
    или старте раунда</b> 
<br>полный пример того, что требуется для работы с Sheduler:
<source lang="cs">









    public Condition OfferBuyVip;
    public Condition OfferToShareWithFriends;
    public Condition OfferToVisitSite;

    public Sheduler OfferSheduler;

    public void Start(){
        OfferSheduler = new Sheduler(currentLevel); // в конструкторе следует указать текущий левел

        /*
         * здесь или в юнити инспекторе заполните поля ваших кондиций
         */

        OfferSheduler.Add(OfferBuyVip, () => Debug.Log("Не желаете ли вы купить VIP"));
        OfferSheduler.Add(OfferToShareWithFriends, () => Debug.Log("Вы можете поделиться с друзьями"));
        OfferSheduler.Add(OfferToVisitSite, () => Debug.Log("Перед игрой лучше посетить наш сайт, там есть подсказки"));
    }

    public void FinishRound(){
        OfferSheduler.NextCheckPoint(currentLevel); // обязательный метод, он также убавляет скипы всем кондициям
        OfferSheduler.Invoke(OfferBuyVip, OfferToShareWithFriends) // будут срабатывать две кондиции при условии, что они на пике
    }

    public void StartRound(){
        OfferSheduler.Invoke(OfferToVisitSite); // на старте раунда будет предлагаться
        //посещение сайта при условии, что кондиция готова, и на этом чек пойнте не было других вызовов от Sheduler
    }
</source>
<hr>
Вот так мы добились того, что три функции наших кондиций вызываются в разных местах, при этом они уважают друг друга и не вылезают все подряд, а соблюдают очередь (как современная цифровая очередь по талончикам), и юзер, быстро перепрыгивая с финиша на старт игры, не будет напрягаться от кол-ва предложений. С Sheduler соблюдает четкую гармонию простоты и гибкости, ведь с Sheduler и делегатом, передаваемым ему через метод <em>Add(Condition newCondition, Action CallBack)</em>, возможно реализовать любую связь между окнами. Например, при вызове рекламного баннера, через два уровня появляется предложение о покупке Премиума без рекламы:

<source lang="cs">




    void Start(){
        OfferSheduler = new Sheduler(currentLevel);

        callAddBanner = new Condition("Вызов Рекламы");
        callAddBanner.setedSeconds = 80; // Кондиция настроенна на 80 секунд
        OfferBuyVip = new Condition("Предложение купить VIP без рекламы");

        OfferSheduler.Add(callAddBanner, 
            delegate()
            {
                Debug.Log("ЗАПУСК РЕКЛАМЫ");
                OfferBuyVip.setedSkips = 2; // Ставим счетчик скипов
                OfferBuyVip.START();  // Запускаем
            }
           );
        OfferSheduler.Add(OfferBuyVip,
            delegate ()
            {
                Debug.Log("Не желаете ли вы купить VIP");
                OfferBuyVip.setedSkips = 0; // Обязательно сбрасываем, запуск рекламы решает, когда запускать
            }
           );
        }
        
        void Finish(){
            OfferSheduler.NextCheckPoint(currentLevel); // не забываем про отсчет пойнтов
            OfferSheduler.Invoke(); // Теперь на финише будут вызываться согласно порядку наши кондиции
        }
</source>
<br>
Вот так вот просто теперь каждые 80 сек будет вызываться не отвлекающая реклама 
(Ведь она вызывается не во время важного раунда, а на финише) и ещё вызывать предложение о покупке рекламы тогда, 
когда вам это удобно! И самое прекрасное, что теперь любой разработчик в команде может добавлять в Sheduler свои предложения, 
и Sheduler все распределит.
