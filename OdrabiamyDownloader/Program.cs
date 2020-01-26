using System;
using System.IO;
using System.Threading.Tasks;
using static OdrabiamyDownloader.UserInput;

namespace OdrabiamyDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("OdrabiamyDownloader - Tool for downloading solutions from Odrabiamy.pl");
            Console.WriteLine("Created by Konrad Krawiec\n");

            Console.WriteLine("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, " +
                "EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, " +
                "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS " +
                "OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, " +
                "WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION " +
                "WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\n");

            Console.WriteLine("Please provide the following data: ");
            string authCookie = GetStringFromUser("Authentication cookie");
            int bookId = GetIntFromUser("Book ID", 1, int.MaxValue);
            string bookName = GetStringFromUser("Book name");
            string subject = GetStringFromUser("Subject");
            int sleepTime = GetIntFromUser("Sleep time", 0, int.MaxValue);
            var odrabiamyClient = new OdrabiamyApiClient(authCookie)
            {
                SleepTime = sleepTime
            };

            Console.WriteLine("Initializing...");
            var pages = await odrabiamyClient.InitializeAndGetPages(subject, bookId);
            Console.WriteLine($"Ready to start. {pages.Length} pages available.");
            Console.WriteLine("Please note that your account may be banned for using this bot.");
            Console.WriteLine("Continue (y/n)? ");
            if (!CanContinue())
                return;

            if (!Directory.Exists(Path.Combine(odrabiamyClient.WorkingDirectory, bookName)))
                Directory.CreateDirectory(Path.Combine(odrabiamyClient.WorkingDirectory, bookName));

            foreach (int page in pages)
            {
                Console.WriteLine($"Downloading solutions for page {page}...");
                await odrabiamyClient.DownloadPageSolutions(subject, bookId, page, bookName);
                Console.WriteLine("Sleeping...");
                await odrabiamyClient.Sleep();
            }
        }
    }
}
