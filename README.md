# EnvelopeGenerator

מערכת ליצירת קבצי מעטפיות מנתוני Access DB.

## מטרת המערכת
המערכת מחליפה את הפונקציונליות הקיימת ב-Access ליצירת קבצי מעטפיות עבור בית הדפוס, תוך שמירה על תאימות מלאה לפורמט הקיים.

## סוגי קבצים
- קובץ שוטף
- קובץ חוב
- קובץ משולב שוטף+חוב

## תלויות
- OdbcConverter.dll - להמרת connection strings
- SQL Server
- .NET 6.0 ומעלה

## שימוש
```csharp
var result = EnvelopeProcessManager.GenerateEnvelopesProcess(
    odbcName: "MyODBC",
    actionType: 1,  // 1=שוטף, 2=חוב, 3=משולב
    envelopeType: 1,
    batchNumber: 202401);
```