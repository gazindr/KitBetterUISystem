# UI System

Собственная UI-система для Unity на базе Odin Inspector. Это UPM-пакет для установки через Unity Package Manager из GitHub/local git repository. Код пакета лежит в `Runtime` и `Editor` и не зависит от DoozyUI.

## Статус проекта

- Odin Inspector должен быть установлен в проекте-получателе до импорта пакета.
- DOTween не найден, поэтому используется внутренний coroutine tween runner.
- UGUI подключается через dependency `com.unity.ugui`.
- Test scene лежит в `Samples~/TestScene` и импортируется через Package Manager samples.
- DoozyUI не используется.
- Runtime компилируется в `Yeen.UISystem.Runtime`, Editor-инспекторы - в `Yeen.UISystem.Editor`.

## Основные компоненты

- `UIButton` - кнопка с pointer/submit/double/long click behaviours.
- `UIToggle` - переключатель с `IsOn`, select/deselect/multiple select и отдельными animation targets для background/handle.
- `UITab` - таб с optional `UITabGroup` и optional linked `UIContainer`.
- `UISlider` - lightweight slider-wrapper с `ValueChanged`, fill/handle targets и без `Update()`.
- `UIContainer` - экран/окно/панель с `Show`, `Hide`, queue, auto-hide, background и callbacks.
- `UIBackground` - fullscreen фон позади `UIContainer`.
- `UIFactoryItem` - editor helper для создания объектов из Hierarchy.

Namespace для кода: `Project.UI`.

## Как быстро создать всю структуру

1. В верхнем меню Unity выбери `GameObject/UI System/Create UI_System`.
2. В Hierarchy появится корень `UI_System`.
3. Внутри будут папки:
   - `UISelectable`
   - `Container`
4. В каждой папке есть стартовые объекты всех типов.
5. В подпапке `Create New` лежат factory-объекты с плюсом:
   - `+ UIButton`
   - `+ UIToggle`
   - `+ UITab`
   - `+ UISlider`
   - `+ UIContainer`
   - `+ Queued UIContainer`
   - `+ UIContainer With Background`

Если выбрать такой `+` объект в Hierarchy, система автоматически создаст новый реальный объект нужного типа и выделит его. Это работает только в Editor.

Также можно создавать объекты напрямую:

- `GameObject/UI System/UIButton`
- `GameObject/UI System/UIToggle`
- `GameObject/UI System/UITab`
- `GameObject/UI System/UISlider`
- `GameObject/UI System/UIContainer`

Почему не prefab-ы: текущая базовая система создается скриптом, потому что генератор всегда использует актуальные компоненты и настройки. Prefab-ы лучше добавлять позже как визуальные шаблоны под конкретный стиль проекта.

## Editor preview

У компонентов есть editor preview без Play Mode:

- `UIContainer`: `Play Show`, `Play Hide`, `Stop`, `Complete`.
- `UIButton`, `UIToggle`, `UITab`, `UISlider`: `Play State`, `Stop`, `Complete`.
- `UIToggle`: дополнительно `Play Select` и `Play Deselect` для background/handle анимаций.
- `Execute Trigger` в инспекторе может запускать behaviours в Edit Mode; delayed entries выполняются через editor delay.

Preview проигрывает Move/Rotate/Scale/Fade во времени через editor runner, а не просто ставит финальное состояние.

## Inspector layout

Инспекторы `UIContainer` и `UISelectable` разделены верхними tabs:

- `Settings`
- `Animations`
- `Behaviours` для selectable-объектов
- `Background` и `Callbacks` для container
- `Presets`
- `Debug`

Внутри `Animations` состояния тоже открываются через tabs. Для каждой анимации `Move`, `Rotate`, `Scale`, `Fade` тоже используются tabs, а не раскрывающиеся панели. Animation tabs показывают иконку слева от названия и центрируют пару иконка+текст внутри tab.

У `UIButton`, `UIToggle`, `UITab`, `UISlider` отдельный top-level раздел `Callbacks` скрыт. Для пользовательской настройки callbacks нужно использовать `Behaviours` и `Unity Event` внутри behaviour entry.

Animation inspector использует адаптивную раскладку: широкие окна показывают поля в две колонки, узкие окна автоматически переводят их в один столбец. Поля не должны вылезать за правый край инспектора.

Для `UIButton`, `UIToggle`, `UITab`, `UISlider` переходы между states работают как интерактивные transitions: новый state стартует от текущего визуального значения, а не прыгает к `From`. Если у нового state нет включенной анимации для свойства, свойство плавно возвращается к baseline/start. Если у нового state есть настройка, переход идет к ее `To` значению.

Кнопки в инспекторе имеют единый смысл по цвету:

- зеленые кнопки добавляют новый элемент;
- красные кнопки удаляют элемент;
- серые кнопки запускают preview/debug-команды и ничего не удаляют.

## Как устроены Behaviours

В системе используется простое правило: один trigger = один behaviour block = одна entry.

Например, если создан block `Pointer Left Click`, второй такой же block создать нельзя. В `Trigger To Add` уже использованный trigger пропадает, а после `Remove Block` снова возвращается в список.

`Add Entry` и `Remove Entry` больше не используются. Если один click должен делать несколько вещей, добавь несколько вызовов в `Unity Event` или несколько `UIBehaviourAction` в `Actions`.

Поля внутри block:

- `Keyboard Key` - опционально. Если не `None`, этот же behaviour срабатывает по `Input.GetKeyDown` в Play Mode (например `Escape` для Close / Continue). Не вызывает отдельно `UIButton.Click()` — только entry/actions этого block.

Поля внутри entry:

- `Enabled` - включает или отключает выполнение entry.
- `Delay` - задержка перед выполнением entry.
- `Execute Once` - выполнить только один раз после включения объекта.
- `Log Execution` - пишет один log в Console, когда entry реально выполнилась. Это только debug-инструмент.
- `Target Container Override` - необязательный контейнер по умолчанию для actions `Show Container`, `Hide Container`, `Toggle Container`, если у action не задан собственный target.
- `Actions` - список `UIBehaviourAction` assets.
- `Unity Event` - обычный UnityEvent, куда можно добавить сколько угодно функций.

## Очередь для selectable-объектов

`UIButton`, `UIToggle`, `UITab` и `UISlider` поддерживают `Use In Queue` в `Settings`.

Если несколько объектов используют один `Queue Group`, их behaviour triggers выполняются последовательно. Например, если нажать кнопку 1 и кнопку 2 почти одновременно, сначала выполнится behaviour кнопки 1, затем behaviour кнопки 2.

`Queue Release Delay` добавляет дополнительную задержку перед запуском следующего элемента очереди. Это полезно, если callback запускает внешнюю анимацию, длительность которой система не может вычислить сама.

## Как добавить UIButton

1. Создай UI GameObject с `RectTransform` и обычным `Graphic`/`Image`.
2. Добавь компонент `UI System/UIButton`.
3. Во вкладке `Settings` настрой `Click Cooldown`, если нужна пауза между кликами.
4. Во вкладке `Animations` настрой анимации для `Normal`, `Highlighted`, `Pressed`, `Selected`, `Disabled`.
5. Во вкладке `Behaviours` выбери trigger, например `Pointer Left Click`, нажми зеленую кнопку `Add Behaviour`.
6. В добавленном block настрой `Unity Event` или `UIBehaviourAction` assets.
7. При желании укажи `Keyboard Key` (например `Space` / `Escape`), чтобы тот же behaviour вызывался с клавиатуры.

Из кода:

```csharp
using Project.UI;

button.ExecuteTrigger(UIBehaviourTrigger.PointerUp);
button.Click();
button.SetInteractable(false);
```

## Как добавить UIToggle

1. Создай объект toggle и добавь `UI System/UIToggle`.
2. Назначь `backgroundTarget` и `handleTarget`.
3. В `States` настрой:
   - `Background Select`
   - `Background Deselect`
   - `Handle Select`
   - `Handle Deselect`
4. Для `MultipleSelect` задай `multipleSelectCount`, например `5`.
5. Добавь behaviour на trigger `MultipleSelect`.

Из кода:

```csharp
using Project.UI;

toggle.SetIsOn(true);
toggle.Toggle();
```

## Как добавить UITab

1. Добавь `UI System/UITab` на объект таба.
2. Если нужен выбор только одного таба, создай родительский объект с `UI System/UITab Group`.
3. Укажи этот group в каждом `UITab` или зарегистрируй их через список `tabs`.
4. Если таб должен открывать экран, назначь `linkedContainer`.
5. Включи `showLinkedContainerOnSelect` и `hideLinkedContainerOnDeselect` при необходимости.

## Как добавить UIContainer

1. Создай UI-панель/окно.
2. Добавь `UI System/UIContainer`.
3. Укажи `id`, например `SettingsUI`.
4. Выбери `startupMode`:
   - `InstantHide` - скрыть сразу.
   - `InstantShow` - показать сразу.
   - `Hide` - запустить hide-анимацию на старте.
   - `Show` - запустить show-анимацию на старте.
5. Во вкладке `Animations` настрой `Show` и `Hide`.

Из кода:

```csharp
using Project.UI;

UIContainer.Show("SettingsUI");
UIContainer.Hide("SettingsUI");
UIContainer.Toggle("SettingsUI");
```

## Как настроить background

1. Открой `UIContainer`.
2. Во вкладке `Background` включи `useBackground`.
3. Если prefab не нужен, оставь `autoCreate = true`.
4. Настрой:
   - `backgroundColor`
   - `backgroundAlpha`
   - `raycastTarget`
   - `closeContainerOnClick`
5. В `animations` настрой отдельные `Show` и `Hide` анимации фона.

Порядок показа:

1. Background show.
2. Container show.

Порядок скрытия:

1. Container hide.
2. Background hide.

Флаги `waitForBackgroundBeforeContainer` и `waitForContainerBeforeBackground` управляют ожиданием между шагами.

## Как настроить queue

1. На `UIContainer` включи `useInQueue`.
2. Укажи `queueGroup`, например `Popup`.
3. При необходимости задай `queueShowDelay` - задержку перед показом этого контейнера после полного скрытия предыдущего.
4. Контейнеры с одинаковым `queueGroup` будут открываться строго по очереди.
5. Контейнеры без `useInQueue` могут открываться параллельно.

Пример:

- `Exit.useInQueue = true`
- `Finish.useInQueue = true`
- `Death.useInQueue = true`
- у всех `queueGroup = Popup`

Если вызвать все три почти одновременно, `Exit` откроется первым. `Finish` не начнет `Show`, пока `Exit` не закончит `Hide` и не перейдет в `Hidden`. `Death` не начнет `Show`, пока `Finish` не перейдет в `Hidden`.

Если у `Finish.queueShowDelay = 0.5`, то после полного `Hidden` у `Exit` система подождет `0.5` секунды и только потом запустит `Finish.Show`.

Важно: очередь сама не закрывает текущий контейнер. Если `Exit` остается `Visible`, то `Finish` будет ждать. Закрытие можно сделать вручную через `Hide`, через `Use Auto Hide`, через кнопку, callback или behaviour action.

## Как создать и применить preset

1. В Project window нажми `Create/UI System/Presets/...`.
2. Выбери нужный тип:
   - `Button Preset`
   - `Toggle Preset`
   - `Tab Preset`
   - `Slider Preset`
   - `Container Preset`
   - `Animation Preset`
   - `Behaviour Preset`
3. Настрой animations, behaviours, callbacks и settings.
4. В компоненте назначь preset во вкладке `Presets` — значения копируются на инстанс, overrides очищаются.
5. Правь поля на компоненте: отличающиеся от пресета подсвечиваются оранжевым и попадают в overrides.
6. ПКМ по полю: **Apply to Preset** / **Revert to Preset**. На вкладке Presets: dirty `*`, **Save** (все поля), **+** (новый asset).

`UIContainer` / `UIButton` больше не используют `presetApplyMask` и отдельный animation preset в инспекторе — один полный пресет.

Чтобы применить только animations без behaviours на других selectables (Toggle/Tab/Slider):

1. В `presetApplyMask.mode` выбери `OnlyAnimations`.
2. Нажми `Apply Preset`.

## Как добавить свой UIBehaviourAction

Создай ScriptableObject и унаследуйся от `UIBehaviourAction`:

```csharp
using UnityEngine;
using Project.UI;

[CreateAssetMenu(menuName = "UI System/Actions/My Action")]
public sealed class MyAction : UIBehaviourAction
{
    public override void Execute(UIBehaviourContext context)
    {
        Debug.Log(context.trigger);
    }
}
```

После этого action asset можно добавить в любой behaviour entry.

## Установка через GitHub Package Manager

Систему можно держать как отдельный UPM-пакет в GitHub-репозитории. В таком виде Unity обновляет пакет через `Window/Package Manager/Add package from git URL`.

Пример URL:

```text
https://github.com/YOUR_NAME/YeenUISystemPackage.git
```

Для конкретной версии лучше использовать git tag:

```text
https://github.com/YOUR_NAME/YeenUISystemPackage.git#v0.1.0
```

Текущая версия системы использует Odin Inspector attributes в runtime-коде, поэтому в проекте-получателе Odin должен быть установлен до импорта пакета. Odin не является обычной UPM dependency этого пакета, потому что обычно устанавливается из Asset Store или локальной лицензированной копии.

Не публикуй Odin Inspector в публичный репозиторий. Если нужен один личный приватный репозиторий с Odin внутри, сначала проверь условия своей Odin-лицензии.

## Runtime safety

- `UIRegistry` хранит containers по id.
- Static `UIContainer.Show/Hide/Toggle` работают только после регистрации контейнера.
- Дубликаты id логируются warning.
- При `Disable/Destroy` активные animations/coroutines останавливаются.
- Fade автоматически добавляет `CanvasGroup`, если его нет.

## Changelog

### 2026-06-30

- User-facing имя системы переименовано в `UI System`, root в `UI_System`, namespace в `Project.UI`.
- Папка системы переименована в `Assets/UISystem`.
- Кастомные инспекторы `UISelectable` и `UIContainer` переведены на верхние tabs.
- `Move`, `Rotate`, `Scale`, `Fade` в инспекторе теперь открываются через tabs.
- `Behaviour Blocks` в selectable-инспекторе теперь выбираются через tabs по trigger.
- Add-кнопки в кастомном инспекторе окрашены в зеленый, remove/delete-кнопки - в красный.
- `Behaviour Blocks` упрощены до правила: один trigger, один block, одна entry.
- `Trigger To Add` больше не показывает triggers, которые уже используются на объекте.
- `Add Entry` и `Remove Entry` убраны из selectable-инспектора.
- Имя behaviour entry теперь фиксируется по trigger и не редактируется вручную.
- `Debug Logging` переименован в `Log Execution` и описан в README.
- `Target Container` переименован в `Target Container Override` и описан в README.
- `UIButton` получил `Click Cooldown` в `Settings`.
- Исправлена адаптивная раскладка animation inspector: поля `Timing` и `Values` больше не съезжают и не обрезаются при разной ширине Inspector.
- Tabs, section headers и command buttons получили увеличенную высоту и единый padding.
- Animation tabs `Move`, `Rotate`, `Scale`, `Fade` получили иконки из `Assets/UISystem`.
- Исправлены interactive state transitions у selectable-объектов: `Highlighted -> Normal` и другие переходы теперь плавно идут от текущего значения к target или baseline/start.
- State transitions у selectable-объектов больше не прыгают на первый кадр `From`, если новая анимация начинается во время предыдущей.
- Добавлена подготовка к GitHub/UPM-пакету: documented git URL install flow и fallback-поиск editor icons из `Assets/UISystem` или `Packages/com.yeen.ui-system`.
- Отдельный раздел `Callbacks` скрыт у selectable-объектов; callbacks остаются внутри behaviour entries.
- Добавлен editor preview runner: Show/Hide/State проигрываются во времени прямо в Edit Mode.
- `Execute Trigger` в кастомном инспекторе поддерживает delayed behaviour entries в Edit Mode.
- `Use In Queue` и `Auto Hide` перенесены в `Settings`.
- `UIContainer` получил `Queue Show Delay` рядом с `Use In Queue`.
- Очередь `UIContainer` теперь резервирует следующий контейнер на время задержки и запускает его только после фактического `Hidden` предыдущего.
- Selectable-объекты получили `Use In Queue`, `Queue Group`, `Queue Release Delay`.
- Добавлен hierarchy generator: `GameObject/UI System/Create UI_System`.
- Добавлены factory-объекты `+ UIButton`, `+ UIToggle`, `+ UITab`, `+ UISlider`, `+ UIContainer`, `+ Queued UIContainer`, `+ UIContainer With Background`.
- Добавлено автоматическое создание объекта при выборе factory-объекта в Hierarchy.
- Добавлен первый полный runtime-набор `UISystem`.
- Добавлены `UIButton`, `UIToggle`, `UITab`, `UITabGroup`, `UISlider`, `UIContainer`, `UIBackground`.
- Добавлены animation runner, behaviour blocks/actions, registry, queue manager и ScriptableObject presets.
- README создан и актуализирован под текущую реализацию.



