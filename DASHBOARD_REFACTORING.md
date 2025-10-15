# Dashboard API - Рефакторинг

## 📊 Новая архитектура Dashboard API

Мы успешно провели рефакторинг Dashboard API, переходя от множественных endpoints к упрощенной архитектуре с двумя основными endpoints.

## 🚀 Новые API Endpoints

### 1. Краткая сводка - `/api/dashboard/summary`
**GET** `/api/dashboard/summary`

Возвращает краткую сводку основных метрик дашборда:

```json
{
  "totalStudents": 245,
  "activeStudents": 198,
  "totalGroups": 42,
  "activeGroups": 38,
  "todayLessons": 15,
  "weeklyLessons": 87,
  "overallAttendanceRate": 78.5,
  "lowPerformanceGroupsCount": 3,
  "unpaidStudentsCount": 12,
  "trialStudentsCount": 8,
  "topTeachersCount": 5,
  "generatedAt": "2024-01-15T10:30:00Z",
  "reportPeriod": "За последние 30 дней"
}
```

### 2. Детальный отчет - `/api/dashboard/detailed`
**GET** `/api/dashboard/detailed`

Возвращает полный детальный отчет со всеми данными:

```json
{
  "studentStats": {
    "totalStudents": 245,
    "activeStudents": 198,
    "newStudentsThisMonth": 15,
    "inactiveStudents": 47
  },
  "groupStats": {
    "totalGroups": 42,
    "activeGroups": 38,
    "averageGroupSize": 8.5,
    "groupsBySubject": [...]
  },
  "lessonStats": {
    "todayLessons": 15,
    "weeklyLessons": 87,
    "monthlyLessons": 342,
    "completedLessons": 89,
    "canceledLessons": 3
  },
  "attendanceStats": {
    "overallAttendanceRate": 78.5,
    "presentStudentsToday": 125,
    "absentStudentsToday": 20,
    "lateStudentsToday": 8,
    "groupAttendanceRates": [...]
  },
  "lowPerformanceGroups": [...],
  "unpaidStudents": [...],
  "trialStudents": [...],
  "topTeachers": [...],
  "latestScheduleUpdate": {...},
  "groupAttendanceRates": [...],
  "generatedAt": "2024-01-15T10:30:00Z",
  "reportPeriod": "За последние 30 дней"
}
```

## 🔄 Фильтрация

Оба endpoint поддерживают дополнительные параметры фильтрации:

```
GET /api/dashboard/summary?organizationId={guid}&dateFrom=2024-01-01&dateTo=2024-01-31
GET /api/dashboard/detailed?organizationId={guid}&dateFrom=2024-01-01&dateTo=2024-01-31
```

## 📋 Legacy Endpoints (Устаревшие)

Старые endpoints остаются доступными для обратной совместимости, но помечены как устаревшие:

- ⚠️ `/api/dashboard/overview` - используйте `/api/dashboard/summary`
- ⚠️ `/api/dashboard/students` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/groups` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/lessons` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/attendance` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/low-performance` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/unpaid-students` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/trial-students` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/top-teachers` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/latest-schedule-update` - используйте `/api/dashboard/detailed`
- ⚠️ `/api/dashboard/groups/{id}/attendance` - используйте `/api/dashboard/detailed`

## 🎯 Преимущества новой архитектуры

1. **Упрощение** - всего 2 основных endpoint вместо 11
2. **Производительность** - параллельная загрузка данных
3. **Консистентность** - единый формат ответа
4. **Гибкость** - выбор между кратким и детальным отчетом
5. **Кэширование** - проще кэшировать 2 endpoint
6. **Обратная совместимость** - старые endpoints работают

## 🔧 Техническая реализация

### Модели данных:
- `DashboardSummaryDto` - краткая сводка (12 ключевых метрик)
- `DashboardDetailedDto` - детальный отчет (все данные + nested объекты)

### Сервисы:
- `IDashboardService.GetDashboardSummaryAsync()` - получение краткой сводки
- `IDashboardService.GetDashboardDetailedAsync()` - получение детального отчета

### Контроллер:
- `DashboardController.GetDashboardSummary()` - endpoint краткой сводки
- `DashboardController.GetDashboardDetailed()` - endpoint детального отчета

## 📈 Статистика метрик

### Краткая сводка включает:
1. 📊 Общее количество студентов/активных студентов
2. 👥 Общее количество групп/активных групп 
3. 📚 Количество уроков на сегодня/неделю
4. 📉 Количество групп с низкой успеваемостью
5. 💰 Количество студентов с неоплаченными уроками
6. 🎯 Количество студентов на пробных уроках
7. 🏆 Средняя посещаемость по центру
8. 👨‍🏫 Количество топ преподавателей

### Детальный отчет включает все выше + детализацию:
- Полная статистика по студентам, группам, урокам
- Детальная информация по посещаемости
- Списки студентов, групп, преподавателей
- Информация о последних обновлениях расписания

## ✅ Результат рефакторинга

✅ **Исправлена проблема с расписанием** - добавлены Include для Group.Subject  
✅ **Создан упрощенный Dashboard API** - 2 основных endpoint  
✅ **Обратная совместимость** - старые endpoints помечены как устаревшие  
✅ **Улучшена производительность** - параллельная загрузка данных  
✅ **Создана масштабируемая архитектура** - легко добавлять новые метрики

Теперь у вас есть простой и мощный Dashboard API с возможностью выбора уровня детализации данных! 🎉