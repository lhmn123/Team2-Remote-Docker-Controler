using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConnectDockerWeb.Controllers
{
    public class CallController : Controller
    {
        // GET: Call
        public ActionResult Index()
        {
            ViewBag.Image = ValueImage();
            return View();
        }
        [HttpPost]
        public ActionResult Start(string nameContainer, string numberCPUs, string numberRAMs)
        {
            Session["Name"] = nameContainer;
            string value = ExecuteCommandSync("docker run -it -d -v C:/Users:/data --name " + nameContainer + " --memory=\"" + numberRAMs + "\" --cpus=\"" + numberCPUs + "\" ubuntu:18.04");

            if (value == "")
            {
                ExecuteCommandSync("docker stop " + nameContainer);
                ExecuteCommandSync("docker run -it -d -v C:/Users:/data --name CLOUDCompile-2 ubuntu:18.04");
            }
            return RedirectToAction("/Index");
        }

        [HttpPost]
        public ActionResult Stop()
        {
            string nameContainer = (string)Session["Name"];
            ExecuteCommandSync("docker stop " + nameContainer);
            ExecuteCommandSync("docker rm /" + nameContainer);
            ExecuteCommandSync("docker stop CLOUDCompile-2");
            ExecuteCommandSync("docker rm /CLOUDCompile-2");
            return RedirectToAction("/Index");
        }
       
        public string ValueImage()
        {
            string valueImage = ExecuteCommandSync("docker images --digests");
            return valueImage;
        }
        [HttpPost]
        public ActionResult SeeStats()
        {
            ExecuteCommandLineWithoutReturn("docker stats ");
            return View("Index");
        }
       
        [HttpPost]
        public ActionResult Test(FormCollection fc)
        {
            string command = fc["code"];
            string test = ExecuteCommandSync(command);
            ViewBag.ip = test;
            return View("Index");
        }

        public ActionResult DeployPortainer()
        {
            //remove container nếu đã tồn tại
            string commandrm1 = ExecuteCommandSync("docker container stop portainer");
            string commandrm2 = ExecuteCommandSync("docker container rm portainer");
            //- Deploy Portainer 
            string command1 = ExecuteCommandSync("docker volume create portainer_data");
            string command2 = ExecuteCommandSync("docker run -d -p 8000:8000 -p 9000:9000 --name=portainer --restart=always -v /var/run/docker.sock:/var/run/docker.sock -v portainer_data:/data portainer/portainer");
            //- Deploy docker stack
            string command3 = ExecuteCommandSync("curl -L https://downloads.portainer.io/portainer-agent-stack.yml -o portainer-agent-stack.yml");
            string command4 = ExecuteCommandSync("docker stack deploy --compose-file=portainer-agent-stack.yml portainer");
            //tạo đường dẫn đến giao diện web portainer
            ViewBag.Link = "http://localhost:9000/";
            return View("Index");
        }

        public ActionResult ClosePortainer()
        {
            //remove container
            string commandrm1 = ExecuteCommandSync("docker container stop portainer");
            string commandrm2 = ExecuteCommandSync("docker container rm portainer");
            return View("Index");
        }

        [HttpPost]

        public string ExecuteCommandSync(object command)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = processStartInfo;
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }

         public string ExecuteCommandLineWithoutReturn (object command)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

                processStartInfo.UseShellExecute = true;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = processStartInfo;
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }


        //public ActionResult CommitDocker(string name, string imageName)
        //{
        //    //chức năng lưu cấu hình container theo tên
        //    try
        //    {
        //        string command = "docker commit " + name + " " + imageName;
        //        string test = ExecuteCommandSync(command);
        //        return View("Index");
        //    }
        //    catch
        //    {

        //        return View("Index");
        //    }
        //    //string test = ExecuteCommandSync("ipconfig");
        //    //ViewBag.ip = test;
        //}



    }
}