# GitHub
Создайте пустой репозиторий NexsusVKVideo и распакуйте пакет в его локальную папку.
Если используете существующий clone, не выполняйте git init и не добавляйте origin повторно.
Для нового каталога:
```powershell
git init -b main
git add .
git commit -m "docs: initialize NexsusVKVideo development kit"
git remote add origin https://github.com/OWNER/NexsusVKVideo.git
git push -u origin main
```
Замените OWNER. Перед git add проверьте содержимое; в исходном пакете нет credentials.
Для действий Codex с GitHub нужен подключённый коннектор с доступом к этому репозиторию либо авторизованный gh. Вход выполняйте самостоятельно, не передавайте токен в чат.
После появления рабочего CI настройте защиту main и обязательный успешный check build. Не включайте автослияние до определения правил проекта.
