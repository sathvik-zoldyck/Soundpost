<div align="center">

<img src="assets/logo.svg" alt="Soundpost" width="120" />

# Soundpost

### مركزٌ واحد. كلُّ صوت.

**وحدة تحكّم واحدة لصوت Windows: بدّل جهاز الإخراج، وامزج كل تطبيق، و*شاهِد* صوتك — كل ذلك في مكان واحد. محلّي أولاً، بلا حساب، بلا تتبُّع.**

[![CI](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml/badge.svg)](https://github.com/sathvik-zoldyck/Soundpost/actions/workflows/ci.yml)
[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Windows 10 / 11](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4?logo=windows&logoColor=white)](#)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](#)
[![Stars](https://img.shields.io/github/stars/sathvik-zoldyck/Soundpost?style=social)](https://github.com/sathvik-zoldyck/Soundpost/stargazers)

**[خارطة الطريق](ROADMAP.md) · [البنية](ARCHITECTURE.md) · [حزمة الإضافات](PLUGIN_SDK.md) · [المساهمة](CONTRIBUTING.md) · [النقاشات](../../discussions)**

[English](README.md) · [简体中文](README.zh.md) · [Español](README.es.md) · [हिन्दी](README.hi.md) · **العربية** · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [日本語](README.ja.md) · [Русский](README.ru.md) · [한국어](README.ko.md)

<br/>

<img src="assets/media/dashboard.png" alt="لوحة Soundpost: قرص مستوى الصوت الرئيسي، ومبدّل أجهزة التشغيل، ومازج لكل تطبيق، ومقاييس إخراج حيّة" width="880" />

</div>

---

<div dir="rtl">

يوزّع Windows صوتك بين نافذة مستوى الصوت، وقائمة الأجهزة، ولوحة التحكّم بالصوت، وحفنة من أدوات الطرف الثالث التي لا يتحدّث بعضها إلى بعض — ولا تتذكّر أيٌّ منها ما أردتَه. **‏Soundpost هو المركز المفقود.** كل جهاز وكل تطبيق يتدفّق إلى وحدة تحكّم واحدة: تبدّل، وتمزج، وتوجّه، فيصل الصوت إلى المخرج الصحيح. وحين تريد الاستماع فحسب، يحوّل المُصوِّر ما يُشغَّل إلى مشهد يستحقّ المشاهدة.

## المزايا

- **تبديل فوري للجهاز.** غيّر مخرج أو مدخل الصوت الافتراضي بنقرة واحدة.
- **مازج لكل تطبيق.** مستوى الصوت والكتم ومقاييس حيّة لكل تطبيق يُصدر صوتًا.
- **قياس إخراج حيّ.** مقاييس ذروة رئيسية ولكل تطبيق بديناميكية واقعية.
- **مُصوِّر الصوت.** سبعة أنماط حيّة — ‏Ribbon وAurora وSpectrum وRadial وOscilloscope وCymatics ووضع صورة مخصّصة — تتفاعل مع صوتك بمعدّل 60 إطارًا في الثانية، مع إمكانية ضبط الحساسية والتنعيم والتوهّج ولوحة الألوان.
- **تراكب بملء الشاشة.** أخرِج المُصوِّر فوق مقطع موسيقي بخلفية صلبة أو معتّمة أو شفّافة تمامًا.
- **اللوحة السريعة.** نافذة منبثقة صغيرة من شريط المهام لما تفعله أثناء اجتماع، دون فتح وحدة التحكّم الكاملة.
- **أربع سِمات.** ‏Indigo وBlack & Red وRich Gold وCherry Blossom — قابلة للتبديل حيًّا من الإعدادات.
- **محلّي وخاص.** بلا حساب، بلا سحابة، بلا تتبُّع. كل شيء يبقى على جهازك.

## شاهِده

</div>

<div align="center">

<img src="assets/media/themes.png" alt="‏Soundpost بسِماته الأربع: Indigo وBlack and Red وRich Gold وCherry Blossom" width="880" />

<sub><b>أربع سِمات، تُبدَّل حيًّا.</b> ‏Indigo وBlack &amp; Red وRich Gold وCherry Blossom.</sub>

<br/><br/>

<img src="assets/media/quick-panel.png" alt="النافذة المنبثقة للوحة السريعة مع مستوى الصوت الرئيسي وتبديل المخرج وعناصر التحكّم لكل تطبيق" width="320" />

<sub><b>اللوحة السريعة.</b> مستوى الصوت الرئيسي، وتبديل المخرج، والكتم لكل تطبيق — مباشرةً من شريط المهام.</sub>

</div>

<div dir="rtl">

## احصل عليه

‏Soundpost موجَّه إلى **Windows 10 و11**. نزّل نسخة من [الإصدارات](../../releases) عند نشر واحدة، أو ابنِه من المصدر:

```bash
git clone https://github.com/sathvik-zoldyck/Soundpost.git
cd Soundpost
dotnet run --project src/Soundpost.App
```

تحتاج إلى [‏.NET 9 SDK](https://dotnet.microsoft.com/download). ينتج سير عمل الإصدار ملفًّا واحدًا مكتفيًا ذاتيًّا باسم `Soundpost.exe` (دون الحاجة إلى تثبيت ‎.NET‎).

## كيف يعمل

كل تطبيق وجهاز يتدفّق إلى مركز واحد؛ توجّهه وتمزجه وتؤتمته، فيصل إلى المخرج الصحيح. طبقةٌ واحدة من Core Audio تغلّف واجهات Windows COM كي لا يلمسها بقيّة التطبيق مباشرةً، ما يُبقي وحدة التحكّم سلسة ومعالجة الصوت معزولة وقابلة للاختبار.

## وسّعه

صُمّم Soundpost ليُضاف إليه.

- **المُصوِّرات.** النمط عبارة عن صنف واحد يطبّق `IVisualizerRenderer` — انظر [visualizers/](visualizers/). اكتبه وسجّله فيظهر في شريط الأنماط.
- **السِمات.** لوحات الألوان قواميس مكتفية ذاتيًّا؛ السمة الجديدة ملفّ جديد مع عيّنة لون.
- **الإضافات.** واجهة إضافات مدفوعة بالأحداث ضمن خارطة الطريق — انظر [PLUGIN_SDK.md](PLUGIN_SDK.md).

## خارطة الطريق

متاح الآن: تبديل الأجهزة، ومازج ومقاييس لكل تطبيق، والمُصوِّر، وشريط المهام واللوحة السريعة، والحفظ، والسِمات. التالي: المشاهد والملفّات الشخصية، وطبقة أتمتة، وتوجيه لكل تطبيق، وتشخيصات بلغة واضحة. الخطة الكاملة في [ROADMAP.md](ROADMAP.md).

## المساهمة

المساهمات مُرحَّب بها، من مُصوِّر جديد إلى إصلاح خطأ. ابدأ من [CONTRIBUTING.md](CONTRIBUTING.md)، أو افتح [مشكلة](../../issues)، أو ألقِ التحية في [النقاشات](../../discussions). إذا كان Soundpost مفيدًا لك، فنجمةٌ واحدة تساعد الآخرين على إيجاده.

## الرخصة

[GPLv3](LICENSE). برمجية حرّة ومفتوحة المصدر.

</div>
