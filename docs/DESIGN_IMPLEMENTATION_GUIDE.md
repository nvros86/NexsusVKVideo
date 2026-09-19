# Design implementation guide
## Reference и приоритет
assets/mockups/MainWindow_Mockup.png — графический ориентир композиции и атмосферы.
Реализовать настоящий XAML: Grid, ItemsControl/ListBox, TextBox, Button, Slider, templates и bindings.
НЕ вставлять PNG целиком как UI, фон окна, кликабельную карту или содержимое WebView.
Визуальный reference создан по описанию до получения логотипа. Утверждённый логотип находится в assets/branding/a_sleek_high_resolution_app_icon_logo_design_on.png; при реализации использовать именно его. Надпись в макете не является заменой фирменного логотипа.
Числа и состояния в этом документе нормативны; иллюстративные отличия генеративного PNG не копировать буквально.

## Координаты и layout
Единицы — WPF device-independent pixels (DIP). Базовый viewport 1600×1000, минимальный 960×640.
Основная сетка: sidebar 224; центральная область *; player panel 400 (включая внутренние поля).
Toolbar: высота 76, spans над center/player, sidebar занимает полную высоту.
Sidebar: padding 16; logo block высота 80; nav margin top 24; элемент высота 48, gap 8, icon 24, текст отступ 12.
Content: padding 24; заголовок 36 по высоте, gap 20; featured высота 280; следующий раздел через 24.
Grid карточек: gap 16; thumbnail 16:9, min card width 208; title max 2 строки, metadata max 1 строка.
При baseline центральная ширина 976 с учётом внешней сетки, полезная ширина 928: 3 карточки по ~298 DIP.
Player: padding 16, video 16:9, controls min 44; заголовок margin top 16; очередь margin top 24.
Списки имеют самостоятельную вертикальную прокрутку; toolbar/sidebar остаются на месте. Избегать вложенной прокрутки одного направления.

## Токены цветов
| Resource key | Значение | Роль |
|---|---|---|
| Color.Background | #080D18 | фон |
| Color.Surface | #101A2B | панели |
| Color.Card | #152238 | карточки |
| Color.Border | #29425D | границы |
| Color.TextPrimary | #EAF2FF | основной текст |
| Color.TextSecondary | #9CAFC8 | metadata |
| Color.Accent | #27D9F2 | выделение |
| Color.AccentBlue | #397BFF | второй акцент |
| Color.Error | #FF7384 | ошибки |
| Color.Success | #5FE0B1 | успех |
Основная кнопка: cyan фон, #07121E текст. Neon только локально: opacity <=0.22, blur <=16.
Не применять blur/glow ко всему окну или каждой карточке. Изображения не определяют цвета UI.

## Spacing, radius, typography
Spacing scale: 4, 8, 12, 16, 24, 32, 48.
Radius: controls 8; card/panel 12; pill 999. Border 1 DIP; focus ring 2 DIP.
Segoe UI, fallback системный sans-serif. Heading 28/36 Semibold; section 22/28 Semibold;
title 16/22 Semibold; body 14/20 Regular; metadata 12/16 Regular; button 14/20 Semibold.
Не уменьшать текст для укладывания: использовать wrapping, ellipsis и доступную полную подпись.
Заголовки в приложении локализуемы, без ручного изменения регистра строк.

## Компоненты
- NavigationItem: icon, label, optional badge; selected left marker 3 DIP и tinted фон.
- SearchBox: height 40, max width 640, leading search, clear, Enter command; debounce 300 ms при live search.
- ProfileButton: avatar 32 и имя; при anonymous — «Войти».
- FeaturedVideo: image с нижним gradient для текста, title и button; без обязательного autoplay.
- VideoCard: thumbnail, duration badge справа снизу, title, creator, metadata, menu. На hover не загружать player.
- PlayerPanel: host официального player, title, favorite toggle и queue. Если provider не даёт управления, не рисовать неработающие дубли controls.
- EmptyState: короткое объяснение и релевантное действие; ErrorState: безопасное описание и retry.
- AI Center: «Скоро» и пояснение; не показывать выдуманные AI результаты.

## Состояния
Каждый интерактивный control: normal, hover (Surface светлее на один токен), pressed (акцентная граница),
focused (cyan ring 2), disabled (opacity 0.45, без click), selected (marker и фон).
Loading: skeleton в фиксированных размерах, reduced-motion — статический; не блокировать весь экран.
Search: idle/loading/results/empty/error; отмена не выводит alert.
Player: idle/loading/playing/paused/unavailable/error; повтор не создаёт второй WebView.
Favorites: selected/unselected/pending/error; при неудаче сохранения вернуть предыдущий state.
Toast не перекрывает controls и имеет доступное сообщение.

## Responsive
- >=1440: sidebar 224, player 400, центральная сетка 3 колонки при доступной ширине.
- 1200–1439: sidebar 72 с иконками+tooltip, player 360; сетка по min width 208.
- 960–1199: sidebar 72; player становится отдельным маршрутом/основной областью, Home показывает 3 или 2 колонки по фактической ширине.
Число колонок = max(1, floor((availableWidth+16)/(208+16))); не растягивать за границы.
Sidebar label collapse сохраняет AutomationProperties.Name. Не скрывать воспроизводящееся видео без явного перехода или остановки.
При высоте <800 контент прокручивается; кнопки не уходят за фиксированную нижнюю панель.
DPI 100/125/150/200%, UseLayoutRounding и SnapsToDevicePixels. Не масштабировать весь интерфейс через Viewbox.

## XAML resources
Планируемая структура App/Resources:
Tokens/Colors.xaml, Tokens/Brushes.xaml, Tokens/Typography.xaml, Tokens/Spacing.xaml;
Styles/Buttons.xaml, Styles/Inputs.xaml, Styles/Navigation.xaml;
Templates/VideoCard.xaml, Templates/States.xaml; Themes/Dark.xaml.
App.xaml объединяет tokens -> styles -> templates -> theme. Использовать DynamicResource для переключаемых theme brushes.
DataTemplate отвечает за модель карточки; ControlTemplate — за состояния контрола.
Не дублировать hex/spacing по Views. Не держать сервисы, HTTP и бизнес-правила в ResourceDictionary.
Логотип — отдельный Image с uniform sizing и оригинальным aspect ratio после получения файла.

## Доступность и проверка
Tab order: nav -> search -> profile -> main content -> player; все действия доступны клавиатурой.
Enter/Space работают по типу control; Escape закрывает overlay; visible focus всегда сохраняется.
Automation names для icon-only actions. Не полагаться только на цвет. Проверить контраст текста/фокуса, в том числе поверх фото.
Анимации 120–180 ms, учитывают reduced motion; не анимировать списки бесконечно.
Снять скриншоты 1600×1000, 1280×800, 960×640 при 100% и проверить 150/200% DPI.
Сравнить композицию с PNG; допустимое отклонение базовых отступов <=2 DIP, не требовать совпадения фото.
Проверить длинный русский текст, пустую ленту, offline, недоступное видео, фокус и resize.
