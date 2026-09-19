# Исходники

Этап 0 создал проекты согласно docs/PROJECT_STRUCTURE.md:

- `NexsusVKVideo.App` — WPF composition root и стартовое окно;
- `NexsusVKVideo.Core` — модели, контракты и доменные правила;
- `NexsusVKVideo.Infrastructure` — будущие реализации I/O-контрактов;
- `NexsusVKVideo.Modules` — будущие прикладные модули.

Исполняемое приложение — минимальная WebView2-оболочка официального сайта `vkvideo.ru`: после запуска в окне остаётся только интерфейс VK Video. Поиск, каталог, рекомендации и вход выполняются самим сайтом VK; NexsusVKVideo не является клиентом VK API и не реализует собственный каталог.
