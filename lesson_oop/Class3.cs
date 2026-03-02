using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//1.Описать класс Hitman
//1.1 Добавить в класс Hitman поле c currentTool типа ITool(интерфейс)
//1.2 Добавить в класс Hitman поле и свойство currentClothSet типа IClothSet (интерфейс)
//1.3 Добавить метод SetNextTools()
//1.4 Добавить метод ChangeClothes(IClothSet clothSet)
//1.5 Добавить в класс Hitman поле allTools типа List<Itool>
//+ инициализировать
//1.6 Добавить в класс Hitman метод UseTool()
//2. Описать смену одежды и использование оружия
//2.1 Описать класс Tool
//2.1.1 Добавить метод Use() с реализацией по умолчанию (virtual) «бах-бах»
//2.1.2 Создать класс Knife, наследующий Tool c переопределением (override) метода Use и поведением «чик-чик»
//2.2 Описать интерфейс IClothSet
//2.2.1 При желании можно добавить пару методов
//2.2.2 Создать >=2 любых реализаций интерфейса.
//3. Клиентский код:
//3.1 Создать экземпляр класса Hitman
//3.2 Сменить оружие на любое другое. Если currentTool пустое, то задать ему начальное значение
//3.3 Создать экземпляр одежды
//3.4 Сменить комплект одежды на созданный в п.3.3
//3.5 Пустить в ход текущее оружие текущим оружием

namespace lesson_oop
{
    internal class Class3
    {
    }
}
