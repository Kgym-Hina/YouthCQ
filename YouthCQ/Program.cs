using System.Text;
using System.Text.Json;
using YouthCQ;

// ----------------开始

// 获取 token
var token = "";
if(File.Exists("token"))
    token = File.ReadAllText("token");
else
{
    Console.WriteLine("Enter your token:");
    token = Console.ReadLine();
}

// 检查新课程
while (true)
{
    var newToken = await RefreshToken(token);
    if (newToken != null)
    {
        token = newToken;
        File.WriteAllText("token", token);
        Console.Out.WriteLine($"Token refreshed: {token}");
    }

    var courses = await GetNewCourseList(token);
    if (courses.Count != 0)
    {
        Console.Out.WriteLine("-----===== Courses =====-----");
        foreach (var course in courses)
        {
            Console.WriteLine($"{course.courseTitle} - {course.courseUrl}");
            
            if (course.learnCondition == "need_learn")
            {
                Console.Out.WriteLine($"Course {course.courseTitle} is {course.learnCondition}");
                var success = await CreateLearnCourse(token, course.id);
                Console.WriteLine(success
                    ? $"Course {course.courseTitle} learnt."
                    : $"Failed to learn course {course.courseTitle}.");
            }
        }
        Console.Out.WriteLine("-----=================-----");
    }
    else
    {
        Console.Out.WriteLine("No course found.");
    }

    Console.Out.WriteLine($"Waiting for next check at {DateTime.Now.AddMinutes(1)}");
    await Task.Delay(1000 * 60);
}

return;
// ----------------结束

async Task<string?> RefreshToken(string oldToken)
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("token", $"{oldToken}");

    var response = await client.GetAsync("https://app.youth.cq.cqyl.org.cn/api/service-sysmgr/LoginController/auth/refreshToken");

    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("token", out var token))
        {
            return token.GetString();
        }
    }
    else
    {
        Console.Out.WriteLine("Failed to refresh token.");
    }

    return null;
}

async Task<List<Models.Course>> GetNewCourseList(string webToken)
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("token", $"{webToken}");

    var payload = new { pageNo = 1, pageSize = 10, query = new { } };
    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    
    var response = await client.PostAsync("https://app.youth.cq.cqyl.org.cn/api/service-app-second/auth/AppYouthStudyHomeController/getNewCourseList", content);

    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        Console.Out.WriteLine("GetNewCourseList: " + json);
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("result", out var result))
        {
            return JsonSerializer.Deserialize<List<Models.Course>>(result.GetRawText())!;
        }
    }
    else
    {
        Console.Out.WriteLine("Failed to get new course list.");
    }

    return null!;
}

async Task<bool> CreateLearnCourse(string token, string courseId)
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("token", $"{token}");

    var content = new StringContent(JsonSerializer.Serialize(new { courseId }), Encoding.UTF8, "application/json");

    var response = await client.PostAsync("https://app.youth.cq.cqyl.org.cn/api/service-app-second/auth/AppQndaxxMyLearn/createLearnCourse", content);

    if (response.IsSuccessStatusCode)
    {
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("success", out var success))
        {
            return success.GetBoolean();
        }
    }

    return false;
}