// See https://aka.ms/new-console-template for more information
HttpClient client = new HttpClient();

string input = "";

while (input != "exit")
{
    Console.Write("Enter a digit : ");
    input = Console.ReadLine();
    string responseBody = await client.GetStringAsync("http://localhost:8080/increment/step?step=1");
    Console.WriteLine(responseBody);
}
