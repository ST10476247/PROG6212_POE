using System;
using System.IO;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace RaceDay.Services
{
    public static class ErdGenerator
    {
        public static void GenerateAll(string baseDir)
        {
            Directory.CreateDirectory(baseDir);

            string pngPath = Path.Combine(baseDir, "RaceDay_ERD.png");
            string pdfPath = Path.Combine(baseDir, "RaceDay_ERD.pdf");
            string drawioPath = Path.Combine(baseDir, "RaceDay_ERD.drawio");
            string svgPath = Path.Combine(baseDir, "RaceDay_ERD.svg");

            try { GenerateDrawIoXml(drawioPath); Console.WriteLine("DrawIO XML generated."); } catch (Exception ex) { Console.WriteLine($"DrawIO Err: {ex.Message}"); }
            try { GenerateSvg(svgPath); Console.WriteLine("SVG generated."); } catch (Exception ex) { Console.WriteLine($"SVG Err: {ex.Message}"); }
            try { GeneratePdf(pdfPath); Console.WriteLine("PDF generated."); } catch (Exception ex) { Console.WriteLine($"PDF Err: {ex.Message}"); }
            try { GeneratePng(pngPath); Console.WriteLine("PNG generated."); } catch (Exception ex) { Console.WriteLine($"PNG Err: {ex.Message}"); }
        }

        public static void GenerateDrawIoXml(string path)
        {
            string xml = @"<?xml opacity=""1.0"" encoding=""UTF-8""?>
<mxfile host=""app.diagrams.net"" modified=""2026-08-03T22:30:00.000Z"" agent=""RaceDay C# Generator"" version=""21.0.0"">
  <diagram id=""RaceDay-ERD"" name=""RaceDay Data Model ERD"">
    <mxGraphModel dx=""1422"" dy=""794"" grid=""1"" gridSize=""10"" guides=""1"" tooltips=""1"" connect=""1"" arrows=""1"" fold=""1"" page=""1"" pageScale=""1"" pageWidth=""1169"" pageHeight=""827"" background=""#ffffff"">
      <root>
        <mxCell id=""0"" />
        <mxCell id=""1"" parent=""0"" />
        
        <!-- Header Title -->
        <mxCell id=""title"" value=""RaceDay - South African Event Management System ERD"" style=""text;html=1;strokeColor=none;fillColor=none;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;fontSize=20;fontStyle=1;fontColor=#1E293B;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""180"" y=""20"" width=""800"" height=""40"" as=""geometry"" />
        </mxCell>

        <!-- Organiser Entity -->
        <mxCell id=""e_organiser"" value=""ORGANISER&#10;-------------------------&#10;PK OrganiserID : Int&#10;OrganizationName : String&#10;ContactEmail : String&#10;Phone : String&#10;Province : String&#10;IsVerified : Boolean"" style=""shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor=#1E293B;fontColor=#FFFFFF;strokeColor=#0F172A;swimlaneFillColor=#F8FAFC;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""40"" y=""100"" width=""220"" height=""180"" as=""geometry"" />
        </mxCell>

        <!-- Event Entity -->
        <mxCell id=""e_event"" value=""EVENT&#10;-------------------------&#10;PK EventID : Int&#10;FK OrganiserID : Int&#10;EventName : String&#10;EventType : Enum (Run/Cycle/Walk)&#10;EventDate : DateTime&#10;Location : String&#10;Province : String&#10;Status : Enum"" style=""shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor=#0284C7;fontColor=#FFFFFF;strokeColor=#0369A1;swimlaneFillColor=#F0F9FF;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""340"" y=""100"" width=""240"" height=""200"" as=""geometry"" />
        </mxCell>

        <!-- Category Entity -->
        <mxCell id=""e_category"" value=""CATEGORY&#10;-------------------------&#10;PK CategoryID : Int&#10;FK EventID : Int&#10;CategoryName : String&#10;DistanceKm : Decimal&#10;EntryFeeZAR : Decimal&#10;MaxCapacity : Int&#10;StartTime : TimeSpan&#10;CutoffHours : Decimal"" style=""shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor=#059669;fontColor=#FFFFFF;strokeColor=#047857;swimlaneFillColor=#ECFDF5;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""660"" y=""100"" width=""220"" height=""200"" as=""geometry"" />
        </mxCell>

        <!-- Participant Entity -->
        <mxCell id=""e_participant"" value=""PARTICIPANT&#10;-------------------------&#10;PK ParticipantID : Int&#10;FirstName : String&#10;LastName : String&#10;SAIDOrPassport : String&#10;Gender : String&#10;ClubName : String&#10;EmergencyPhone : String&#10;Email : String"" style=""shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor=#D97706;fontColor=#FFFFFF;strokeColor=#B45309;swimlaneFillColor=#FFFBEB;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""660"" y=""400"" width=""220"" height=""200"" as=""geometry"" />
        </mxCell>

        <!-- Entry Entity -->
        <mxCell id=""e_entry"" value=""ENTRY (Associative)&#10;-------------------------&#10;PK EntryID : Int&#10;FK ParticipantID : Int&#10;FK CategoryID : Int&#10;BibNumber : String&#10;RegistrationDate : DateTime&#10;PaymentStatus : Enum&#10;PaymentReference : String"" style=""shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor=#7C3AED;fontColor=#FFFFFF;strokeColor=#6D28D9;swimlaneFillColor=#F5F3FF;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""340"" y=""400"" width=""240"" height=""190"" as=""geometry"" />
        </mxCell>

        <!-- Result Entity -->
        <mxCell id=""e_result"" value=""RESULT&#10;-------------------------&#10;PK ResultID : Int&#10;FK EntryID : Int (Unique)&#10;GunTime : TimeSpan&#10;ChipTime : TimeSpan&#10;OverallRank : Int&#10;CategoryRank : Int&#10;GenderRank : Int&#10;Status : Enum (Finished/DNF)"" style=""shape=swimlane;fontStyle=1;align=center;verticalAlign=top;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;whiteSpace=wrap;html=1;fillColor=#DC2626;fontColor=#FFFFFF;strokeColor=#B91C1C;swimlaneFillColor=#FEF2F2;"" vertex=""1"" parent=""1"">
          <mxGeometry x=""40"" y=""400"" width=""220"" height=""190"" as=""geometry"" />
        </mxCell>

        <!-- Relationships -->
        <mxCell id=""r1"" value=""1 : N (Organises)"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.5;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;strokeWidth=2;strokeColor=#475569;fontSize=12;fontStyle=1;"" edge=""1"" parent=""1"" source=""e_organiser"" target=""e_event"">
          <mxGeometry relative=""1"" as=""geometry"" />
        </mxCell>

        <mxCell id=""r2"" value=""1 : N (Has Categories)"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=1;exitY=0.5;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;strokeWidth=2;strokeColor=#475569;fontSize=12;fontStyle=1;"" edge=""1"" parent=""1"" source=""e_event"" target=""e_category"">
          <mxGeometry relative=""1"" as=""geometry"" />
        </mxCell>

        <mxCell id=""r3"" value=""1 : N (Has Registrations)"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=1;entryY=0.5;entryDx=0;entryDy=0;strokeWidth=2;strokeColor=#475569;fontSize=12;fontStyle=1;"" edge=""1"" parent=""1"" source=""e_category"" target=""e_entry"">
          <mxGeometry relative=""1"" as=""geometry"" />
        </mxCell>

        <mxCell id=""r4"" value=""1 : N (Registers)"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0;exitY=0.5;exitDx=0;exitDy=0;entryX=1;entryY=0.7;entryDx=0;entryDy=0;strokeWidth=2;strokeColor=#475569;fontSize=12;fontStyle=1;"" edge=""1"" parent=""1"" source=""e_participant"" target=""e_entry"">
          <mxGeometry relative=""1"" as=""geometry"" />
        </mxCell>

        <mxCell id=""r5"" value=""1 : 0..1 (Generates)"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;exitX=0;exitY=0.5;exitDx=0;exitDy=0;entryX=1;entryY=0.5;entryDx=0;entryDy=0;strokeWidth=2;strokeColor=#475569;fontSize=12;fontStyle=1;"" edge=""1"" parent=""1"" source=""e_entry"" target=""e_result"">
          <mxGeometry relative=""1"" as=""geometry"" />
        </mxCell>

      </root>
    </mxGraphModel>
  </diagram>
</mxfile>";

            File.WriteAllText(path, xml, Encoding.UTF8);
        }

        public static void GenerateSvg(string path)
        {
            string svg = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 1200 800"" width=""1200"" height=""800"" style=""background-color: #F8FAFC; font-family: system-ui, -apple-system, sans-serif;"">
  <defs>
    <filter id=""shadow"" x=""-5%"" y=""-5%"" width=""110%"" height=""110%"">
      <feDropShadow dx=""2"" dy=""4"" stdDeviation=""4"" flood-opacity=""0.15""/>
    </filter>
    <marker id=""arrow"" viewBox=""0 0 10 10"" refX=""6"" refY=""5"" markerWidth=""8"" markerHeight=""8"" orient=""auto-start-reverse"">
      <path d=""M 0 1 L 10 5 L 0 9 z"" fill=""#475569"" />
    </marker>
  </defs>

  <!-- Title & Subtitle Banner -->
  <rect x=""40"" y=""25"" width=""1120"" height=""70"" rx=""12"" fill=""#1E293B"" filter=""url(#shadow)""/>
  <text x=""560"" y=""55"" fill=""#F8FAFC"" font-size=""24"" font-weight=""800"" text-anchor=""middle"">RaceDay - South African Event Management System</text>
  <text x=""560"" y=""78"" fill=""#94A3B8"" font-size=""14"" font-weight=""500"" text-anchor=""middle"">Entity-Relationship Diagram (ERD) • Primary Keys (PK), Foreign Keys (FK) &amp; Cardinalities</text>

  <!-- Legend -->
  <rect x=""950"" y=""35"" width=""190"" height=""50"" rx=""6"" fill=""#0F172A"" opacity=""0.8""/>
  <text x=""960"" y=""53"" fill=""#F59E0B"" font-size=""11"" font-weight=""700"">[PK] Primary Key</text>
  <text x=""960"" y=""70"" fill=""#38BDF8"" font-size=""11"" font-weight=""700"">[FK] Foreign Key</text>

  <!-- ENTITY 1: ORGANISER -->
  <g filter=""url(#shadow)"">
    <rect x=""50"" y=""130"" width=""240"" height=""230"" rx=""10"" fill=""#FFFFFF"" stroke=""#CBD5E1"" stroke-width=""2""/>
    <rect x=""50"" y=""130"" width=""240"" height=""40"" rx=""10"" fill=""#1E293B""/>
    <rect x=""50"" y=""160"" width=""240"" height=""10"" fill=""#1E293B""/>
    <text x=""170"" y=""156"" fill=""#FFFFFF"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">ORGANISER</text>
    
    <text x=""65"" y=""190"" fill=""#F59E0B"" font-size=""13"" font-weight=""700"">PK OrganiserID</text><text x=""180"" y=""190"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""65"" y=""212"" fill=""#334155"" font-size=""13"">OrganizationName</text><text x=""195"" y=""212"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""65"" y=""234"" fill=""#334155"" font-size=""13"">ContactEmail</text><text x=""170"" y=""234"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""65"" y=""256"" fill=""#334155"" font-size=""13"">Phone</text><text x=""135"" y=""256"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""65"" y=""278"" fill=""#334155"" font-size=""13"">Province</text><text x=""145"" y=""278"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""65"" y=""300"" fill=""#334155"" font-size=""13"">IsVerified</text><text x=""155"" y=""300"" fill=""#64748B"" font-size=""12"">: Bool</text>
    <text x=""65"" y=""322"" fill=""#334155"" font-size=""13"">CreatedAt</text><text x=""150"" y=""322"" fill=""#64748B"" font-size=""12"">: DateTime</text>
  </g>

  <!-- ENTITY 2: EVENT -->
  <g filter=""url(#shadow)"">
    <rect x=""360"" y=""130"" width=""260"" height=""270"" rx=""10"" fill=""#FFFFFF"" stroke=""#CBD5E1"" stroke-width=""2""/>
    <rect x=""360"" y=""130"" width=""260"" height=""40"" rx=""10"" fill=""#0284C7""/>
    <rect x=""360"" y=""160"" width=""260"" height=""10"" fill=""#0284C7""/>
    <text x=""490"" y=""156"" fill=""#FFFFFF"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">EVENT</text>
    
    <text x=""375"" y=""190"" fill=""#F59E0B"" font-size=""13"" font-weight=""700"">PK EventID</text><text x=""475"" y=""190"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""375"" y=""212"" fill=""#0284C7"" font-size=""13"" font-weight=""700"">FK OrganiserID</text><text x=""495"" y=""212"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""375"" y=""234"" fill=""#334155"" font-size=""13"">EventName</text><text x=""465"" y=""234"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""375"" y=""256"" fill=""#334155"" font-size=""13"">EventType</text><text x=""455"" y=""256"" fill=""#64748B"" font-size=""12"">: Enum</text>
    <text x=""375"" y=""278"" fill=""#334155"" font-size=""13"">EventDate</text><text x=""460"" y=""278"" fill=""#64748B"" font-size=""12"">: DateTime</text>
    <text x=""375"" y=""300"" fill=""#334155"" font-size=""13"">Location</text><text x=""450"" y=""300"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""375"" y=""322"" fill=""#334155"" font-size=""13"">Province</text><text x=""455"" y=""322"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""375"" y=""344"" fill=""#334155"" font-size=""13"">Status</text><text x=""435"" y=""344"" fill=""#64748B"" font-size=""12"">: Enum</text>
    <text x=""375"" y=""366"" fill=""#334155"" font-size=""13"">BannerUrl</text><text x=""460"" y=""366"" fill=""#64748B"" font-size=""12"">: String</text>
  </g>

  <!-- ENTITY 3: CATEGORY -->
  <g filter=""url(#shadow)"">
    <rect x=""690"" y=""130"" width=""250"" height=""270"" rx=""10"" fill=""#FFFFFF"" stroke=""#CBD5E1"" stroke-width=""2""/>
    <rect x=""690"" y=""130"" width=""250"" height=""40"" rx=""10"" fill=""#059669""/>
    <rect x=""690"" y=""160"" width=""250"" height=""10"" fill=""#059669""/>
    <text x=""815"" y=""156"" fill=""#FFFFFF"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">CATEGORY</text>
    
    <text x=""705"" y=""190"" fill=""#F59E0B"" font-size=""13"" font-weight=""700"">PK CategoryID</text><text x=""815"" y=""190"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""705"" y=""212"" fill=""#0284C7"" font-size=""13"" font-weight=""700"">FK EventID</text><text x=""800"" y=""212"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""705"" y=""234"" fill=""#334155"" font-size=""13"">CategoryName</text><text x=""815"" y=""234"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""256"" fill=""#334155"" font-size=""13"">DistanceKm</text><text x=""800"" y=""256"" fill=""#64748B"" font-size=""12"">: Decimal</text>
    <text x=""705"" y=""278"" fill=""#334155"" font-size=""13"">EntryFeeZAR</text><text x=""805"" y=""278"" fill=""#64748B"" font-size=""12"">: Decimal</text>
    <text x=""705"" y=""300"" fill=""#334155"" font-size=""13"">MaxCapacity</text><text x=""800"" y=""300"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""705"" y=""322"" fill=""#334155"" font-size=""13"">StartTime</text><text x=""785"" y=""322"" fill=""#64748B"" font-size=""12"">: TimeSpan</text>
    <text x=""705"" y=""344"" fill=""#334155"" font-size=""13"">CutoffHours</text><text x=""800"" y=""344"" fill=""#64748B"" font-size=""12"">: Decimal</text>
  </g>

  <!-- ENTITY 4: PARTICIPANT -->
  <g filter=""url(#shadow)"">
    <rect x=""690"" y=""470"" width=""250"" height=""270"" rx=""10"" fill=""#FFFFFF"" stroke=""#CBD5E1"" stroke-width=""2""/>
    <rect x=""690"" y=""470"" width=""250"" height=""40"" rx=""10"" fill=""#D97706""/>
    <rect x=""690"" y=""500"" width=""250"" height=""10"" fill=""#D97706""/>
    <text x=""815"" y=""496"" fill=""#FFFFFF"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">PARTICIPANT</text>
    
    <text x=""705"" y=""530"" fill=""#F59E0B"" font-size=""13"" font-weight=""700"">PK ParticipantID</text><text x=""830"" y=""530"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""705"" y=""552"" fill=""#334155"" font-size=""13"">FirstName</text><text x=""785"" y=""552"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""574"" fill=""#334155"" font-size=""13"">LastName</text><text x=""785"" y=""574"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""596"" fill=""#334155"" font-size=""13"">SAIDOrPassport</text><text x=""825"" y=""596"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""618"" fill=""#334155"" font-size=""13"">Gender</text><text x=""770"" y=""618"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""640"" fill=""#334155"" font-size=""13"">ClubName</text><text x=""785"" y=""640"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""662"" fill=""#334155"" font-size=""13"">EmergencyPhone</text><text x=""825"" y=""662"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""705"" y=""684"" fill=""#334155"" font-size=""13"">Email</text><text x=""755"" y=""684"" fill=""#64748B"" font-size=""12"">: String</text>
  </g>

  <!-- ENTITY 5: ENTRY (Associative Entity) -->
  <g filter=""url(#shadow)"">
    <rect x=""360"" y=""470"" width=""260"" height=""270"" rx=""10"" fill=""#FFFFFF"" stroke=""#CBD5E1"" stroke-width=""2""/>
    <rect x=""360"" y=""470"" width=""260"" height=""40"" rx=""10"" fill=""#7C3AED""/>
    <rect x=""360"" y=""500"" width=""260"" height=""10"" fill=""#7C3AED""/>
    <text x=""490"" y=""496"" fill=""#FFFFFF"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">ENTRY (Associative)</text>
    
    <text x=""375"" y=""530"" fill=""#F59E0B"" font-size=""13"" font-weight=""700"">PK EntryID</text><text x=""465"" y=""530"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""375"" y=""552"" fill=""#0284C7"" font-size=""13"" font-weight=""700"">FK ParticipantID</text><text x=""500"" y=""552"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""375"" y=""574"" fill=""#0284C7"" font-size=""13"" font-weight=""700"">FK CategoryID</text><text x=""485"" y=""574"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""375"" y=""596"" fill=""#334155"" font-size=""13"">BibNumber</text><text x=""465"" y=""596"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""375"" y=""618"" fill=""#334155"" font-size=""13"">RegistrationDate</text><text x=""500"" y=""618"" fill=""#64748B"" font-size=""12"">: DateTime</text>
    <text x=""375"" y=""640"" fill=""#334155"" font-size=""13"">PaymentStatus</text><text x=""485"" y=""640"" fill=""#64748B"" font-size=""12"">: Enum</text>
    <text x=""375"" y=""662"" fill=""#334155"" font-size=""13"">PaymentReference</text><text x=""510"" y=""662"" fill=""#64748B"" font-size=""12"">: String</text>
    <text x=""375"" y=""684"" fill=""#334155"" font-size=""13"">MedicalNotes</text><text x=""475"" y=""684"" fill=""#64748B"" font-size=""12"">: String</text>
  </g>

  <!-- ENTITY 6: RESULT -->
  <g filter=""url(#shadow)"">
    <rect x=""50"" y=""470"" width=""240"" height=""270"" rx=""10"" fill=""#FFFFFF"" stroke=""#CBD5E1"" stroke-width=""2""/>
    <rect x=""50"" y=""470"" width=""240"" height=""40"" rx=""10"" fill=""#DC2626""/>
    <rect x=""50"" y=""500"" width=""240"" height=""10"" fill=""#DC2626""/>
    <text x=""170"" y=""496"" fill=""#FFFFFF"" font-size=""16"" font-weight=""700"" text-anchor=""middle"">RESULT</text>
    
    <text x=""65"" y=""530"" fill=""#F59E0B"" font-size=""13"" font-weight=""700"">PK ResultID</text><text x=""160"" y=""530"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""65"" y=""552"" fill=""#0284C7"" font-size=""13"" font-weight=""700"">FK EntryID (Unique)</text><text x=""205"" y=""552"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""65"" y=""574"" fill=""#334155"" font-size=""13"">GunTime</text><text x=""140"" y=""574"" fill=""#64748B"" font-size=""12"">: TimeSpan</text>
    <text x=""65"" y=""596"" fill=""#334155"" font-size=""13"">ChipTime</text><text x=""140"" y=""596"" fill=""#64748B"" font-size=""12"">: TimeSpan</text>
    <text x=""65"" y=""618"" fill=""#334155"" font-size=""13"">OverallRank</text><text x=""155"" y=""618"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""65"" y=""640"" fill=""#334155"" font-size=""13"">CategoryRank</text><text x=""165"" y=""640"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""65"" y=""662"" fill=""#334155"" font-size=""13"">GenderRank</text><text x=""160"" y=""662"" fill=""#64748B"" font-size=""12"">: Int</text>
    <text x=""65"" y=""684"" fill=""#334155"" font-size=""13"">Status</text><text x=""120"" y=""684"" fill=""#64748B"" font-size=""12"">: Enum</text>
  </g>

  <!-- RELATIONSHIP LINES & CARDINALITIES -->

  <!-- R1: ORGANISER (1) ---> (N) EVENT -->
  <path d=""M 290 230 L 360 230"" stroke=""#475569"" stroke-width=""3.5"" marker-end=""url(#arrow)""/>
  <rect x=""300"" y=""208"" width=""45"" height=""20"" rx=""4"" fill=""#1E293B""/>
  <text x=""322"" y=""222"" fill=""#FFFFFF"" font-size=""12"" font-weight=""800"" text-anchor=""middle"">1 : N</text>
  <text x=""325"" y=""245"" fill=""#0F172A"" font-size=""11"" font-weight=""700"" text-anchor=""middle"">Organises</text>

  <!-- R2: EVENT (1) ---> (N) CATEGORY -->
  <path d=""M 620 230 L 690 230"" stroke=""#475569"" stroke-width=""3.5"" marker-end=""url(#arrow)""/>
  <rect x=""630"" y=""208"" width=""45"" height=""20"" rx=""4"" fill=""#0284C7""/>
  <text x=""652"" y=""222"" fill=""#FFFFFF"" font-size=""12"" font-weight=""800"" text-anchor=""middle"">1 : N</text>
  <text x=""655"" y=""245"" fill=""#0F172A"" font-size=""11"" font-weight=""700"" text-anchor=""middle"">Has Categories</text>

  <!-- R3: CATEGORY (1) ---> (N) ENTRY -->
  <path d=""M 815 400 L 815 435 L 490 435 L 490 470"" stroke=""#475569"" stroke-width=""3.5"" fill=""none"" marker-end=""url(#arrow)""/>
  <rect x=""625"" y=""424"" width=""45"" height=""20"" rx=""4"" fill=""#059669""/>
  <text x=""647"" y=""438"" fill=""#FFFFFF"" font-size=""12"" font-weight=""800"" text-anchor=""middle"">1 : N</text>
  <text x=""647"" y=""420"" fill=""#0F172A"" font-size=""11"" font-weight=""700"" text-anchor=""middle"">Category Registrations</text>

  <!-- R4: PARTICIPANT (1) ---> (N) ENTRY -->
  <path d=""M 690 605 L 620 605"" stroke=""#475569"" stroke-width=""3.5"" marker-end=""url(#arrow)""/>
  <rect x=""630"" y=""583"" width=""45"" height=""20"" rx=""4"" fill=""#D97706""/>
  <text x=""652"" y=""597"" fill=""#FFFFFF"" font-size=""12"" font-weight=""800"" text-anchor=""middle"">1 : N</text>
  <text x=""655"" y=""622"" fill=""#0F172A"" font-size=""11"" font-weight=""700"" text-anchor=""middle"">Registers</text>

  <!-- R5: ENTRY (1) ---> (0..1) RESULT -->
  <path d=""M 360 605 L 290 605"" stroke=""#475569"" stroke-width=""3.5"" marker-end=""url(#arrow)""/>
  <rect x=""295"" y=""583"" width=""55"" height=""20"" rx=""4"" fill=""#7C3AED""/>
  <text x=""322"" y=""597"" fill=""#FFFFFF"" font-size=""12"" font-weight=""800"" text-anchor=""middle"">1 : 0..1</text>
  <text x=""325"" y=""622"" fill=""#0F172A"" font-size=""11"" font-weight=""700"" text-anchor=""middle"">Generates</text>

</svg>";

            File.WriteAllText(path, svg, Encoding.UTF8);
        }

        public static void GeneratePdf(string path)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "RaceDay ERD Data Model";
            document.Info.Subject = "South African Road Events Management System";

            PdfPage page = document.AddPage();
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
            page.Width = XUnit.FromPoint(842); // A4 Landscape width
            page.Height = XUnit.FromPoint(595); // A4 Landscape height

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Title
            XFont titleFont = new XFont("Arial", 16, XFontStyle.Bold);
            XFont subtitleFont = new XFont("Arial", 10, XFontStyle.Regular);
            XFont headerFont = new XFont("Arial", 10, XFontStyle.Bold);
            XFont bodyFont = new XFont("Arial", 8, XFontStyle.Regular);

            gfx.DrawRectangle(XBrushes.SlateGray, 20, 20, 802, 50);
            gfx.DrawString("RaceDay - South African Event Management System ERD", titleFont, XBrushes.White, new XRect(20, 25, 802, 25), XStringFormats.TopCenter);
            gfx.DrawString("Entity-Relationship Diagram Specification • Primary Keys (PK), Foreign Keys (FK) & Cardinalities", subtitleFont, XBrushes.LightGray, new XRect(20, 50, 802, 20), XStringFormats.TopCenter);

            // Draw Entity Boxes
            DrawPdfEntity(gfx, "ORGANISER", 30, 90, 220, 190, XBrushes.DarkSlateGray, new[]
            {
                "[PK] OrganiserID : Int",
                "OrganizationName : String",
                "ContactEmail : String",
                "Phone : String",
                "Province : String",
                "IsVerified : Boolean",
                "CreatedAt : DateTime"
            }, headerFont, bodyFont);

            DrawPdfEntity(gfx, "EVENT", 290, 90, 240, 210, XBrushes.DeepSkyBlue, new[]
            {
                "[PK] EventID : Int",
                "[FK] OrganiserID : Int",
                "EventName : String",
                "EventType : Enum (Run/Cycle/Walk)",
                "EventDate : DateTime",
                "Location : String (e.g. Comrades)",
                "Province : String",
                "Status : Enum (Upcoming/Live/Completed)"
            }, headerFont, bodyFont);

            DrawPdfEntity(gfx, "CATEGORY", 560, 90, 240, 210, XBrushes.SeaGreen, new[]
            {
                "[PK] CategoryID : Int",
                "[FK] EventID : Int",
                "CategoryName : String (e.g. 90km Down Run)",
                "DistanceKm : Decimal",
                "EntryFeeZAR : Decimal",
                "MaxCapacity : Int",
                "StartTime : TimeSpan",
                "CutoffHours : Decimal"
            }, headerFont, bodyFont);

            DrawPdfEntity(gfx, "PARTICIPANT", 560, 340, 240, 210, XBrushes.DarkOrange, new[]
            {
                "[PK] ParticipantID : Int",
                "FirstName : String",
                "LastName : String",
                "SAIDOrPassport : String",
                "Gender : String",
                "ClubName : String",
                "EmergencyPhone : String",
                "Email : String"
            }, headerFont, bodyFont);

            DrawPdfEntity(gfx, "ENTRY (Associative)", 290, 340, 240, 210, XBrushes.DarkOrchid, new[]
            {
                "[PK] EntryID : Int",
                "[FK] ParticipantID : Int",
                "[FK] CategoryID : Int",
                "BibNumber : String",
                "RegistrationDate : DateTime",
                "PaymentStatus : Enum",
                "PaymentReference : String",
                "MedicalNotes : String"
            }, headerFont, bodyFont);

            DrawPdfEntity(gfx, "RESULT", 30, 340, 220, 210, XBrushes.Crimson, new[]
            {
                "[PK] ResultID : Int",
                "[FK] EntryID : Int (Unique)",
                "GunTime : TimeSpan",
                "ChipTime : TimeSpan",
                "OverallRank : Int",
                "CategoryRank : Int",
                "GenderRank : Int",
                "Status : Enum (Finished/DNF)"
            }, headerFont, bodyFont);

            // Draw Relationship Connector Lines
            XPen linePen = new XPen(XColors.SlateGray, 2);
            gfx.DrawLine(linePen, 250, 180, 290, 180); // Organiser -> Event
            gfx.DrawString("1 : N (Organises)", bodyFont, XBrushes.DarkSlateGray, 243, 172);

            gfx.DrawLine(linePen, 530, 180, 560, 180); // Event -> Category
            gfx.DrawString("1 : N (Has Categories)", bodyFont, XBrushes.DarkSlateGray, 510, 172);

            gfx.DrawLine(linePen, 680, 300, 680, 320); // Category -> Entry (via vertical drop)
            gfx.DrawLine(linePen, 680, 320, 410, 320);
            gfx.DrawLine(linePen, 410, 320, 410, 340);
            gfx.DrawString("1 : N (Registrations)", bodyFont, XBrushes.DarkSlateGray, 510, 315);

            gfx.DrawLine(linePen, 560, 445, 530, 445); // Participant -> Entry
            gfx.DrawString("1 : N (Registers)", bodyFont, XBrushes.DarkSlateGray, 535, 437);

            gfx.DrawLine(linePen, 290, 445, 250, 445); // Entry -> Result
            gfx.DrawString("1 : 0..1 (Generates)", bodyFont, XBrushes.DarkSlateGray, 245, 437);

            document.Save(path);
        }

        private static void DrawPdfEntity(XGraphics gfx, string title, double x, double y, double w, double h, XBrush color, string[] fields, XFont headerFont, XFont bodyFont)
        {
            gfx.DrawRectangle(XPens.Gray, XBrushes.White, x, y, w, h);
            gfx.DrawRectangle(color, x, y, w, 24);
            gfx.DrawString(title, headerFont, XBrushes.White, new XRect(x, y + 4, w, 20), XStringFormats.TopCenter);

            double curY = y + 32;
            foreach (var f in fields)
            {
                XBrush textBrush = XBrushes.DarkSlateGray;
                if (f.StartsWith("[PK]")) textBrush = XBrushes.DarkGoldenrod;
                else if (f.StartsWith("[FK]")) textBrush = XBrushes.DodgerBlue;

                gfx.DrawString(f, bodyFont, textBrush, x + 8, curY);
                curY += 18;
            }
        }

        public static void GeneratePng(string path)
        {
            int width = 1200;
            int height = 800;

            using (var image = new Image<Rgba32>(width, height))
            {
                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.ParseHex("#F8FAFC"));
                    ctx.Fill(Color.ParseHex("#1E293B"), new RectangleF(40, 25, 1120, 70));
                    ctx.Fill(Color.ParseHex("#0F172A"), new RectangleF(950, 35, 190, 50));

                    DrawPngEntity(ctx, "ORGANISER", 50, 130, 240, 230, "#1E293B");
                    DrawPngEntity(ctx, "EVENT", 360, 130, 260, 270, "#0284C7");
                    DrawPngEntity(ctx, "CATEGORY", 690, 130, 250, 270, "#059669");
                    DrawPngEntity(ctx, "PARTICIPANT", 690, 470, 250, 270, "#D97706");
                    DrawPngEntity(ctx, "ENTRY (Associative)", 360, 470, 260, 270, "#7C3AED");
                    DrawPngEntity(ctx, "RESULT", 50, 470, 240, 270, "#DC2626");

                    var linePen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(Color.ParseHex("#475569"), 3);
                    ctx.DrawLine(linePen, new PointF(290, 230), new PointF(360, 230)); // R1
                    ctx.DrawLine(linePen, new PointF(620, 230), new PointF(690, 230)); // R2
                    ctx.DrawLine(linePen, new PointF(815, 400), new PointF(815, 435)); // R3 part 1
                    ctx.DrawLine(linePen, new PointF(815, 435), new PointF(490, 435)); // R3 part 2
                    ctx.DrawLine(linePen, new PointF(490, 435), new PointF(490, 470)); // R3 part 3
                    ctx.DrawLine(linePen, new PointF(690, 605), new PointF(620, 605)); // R4
                    ctx.DrawLine(linePen, new PointF(360, 605), new PointF(290, 605)); // R5
                });

                image.SaveAsPng(path);
            }
        }

        private static void DrawPngEntity(IImageProcessingContext ctx, string title, float x, float y, float w, float h, string headerColorHex)
        {
            ctx.Fill(Color.White, new RectangleF(x, y, w, h));
            ctx.Draw(SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(Color.ParseHex("#CBD5E1"), 2), new RectangleF(x, y, w, h));
            ctx.Fill(Color.ParseHex(headerColorHex), new RectangleF(x, y, w, 40));
        }
    }
}
