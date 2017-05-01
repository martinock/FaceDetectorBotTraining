using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceDetector
{
    class Program
    {
        FaceServiceClient faceServiceClient = new FaceServiceClient("5388739dcaf64c4e8fd2a5a37a132788");
        private static string PERSON_GROUP_IF = "ifitb";

        public async void CreatePersonGroup(string personGroupId, string personGroupName)
        {
            try
            {
                await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Create Error : " + ex.Message);
            }
        }

        public async void AddPersonToGroup(string personGroupId, string name, string imagePath)
        {
            try
            {
                await faceServiceClient.GetPersonGroupAsync(personGroupId);
                CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupId, name);

                DetectFaceAndRegister(personGroupId, person, imagePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Add Error : " + ex.Message);
            }
        }

        private async void DetectFaceAndRegister(string personGroupId, CreatePersonResult person, string imagePath)
        {
            foreach (var imgPath in Directory.GetFiles(imagePath, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imgPath))
                {
                    await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);
                }
            }
        }

        public async void TrainingAI(string personGroupId)
        {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
            TrainingStatus trainingStatus = null;

            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);
                if (trainingStatus.Status != Status.Running)
                {
                    break;
                }
                await Task.Delay(1000);
            }
            Console.WriteLine("Training AI completed");
        }

        public async void RecognitionFace (string personGroupId, string imagePath)
        {
            using (Stream s = File.OpenRead(imagePath))
            {
                var faces = await faceServiceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                try
                {
                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    foreach(var identifyResult in results)
                    {
                        Console.WriteLine($"Result of face: {identifyResult.FaceId}");
                        if (identifyResult.Candidates.Length == 0)
                        {
                            Console.WriteLine("No one found");
                        } else
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                            Console.WriteLine($"Identified as {person.Name}");
                        }
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine("Recognition Error : " + ex.Message);
                }
            }
        }

        static void Main(string[] args)
        {
            /*new Program().CreatePersonGroup(PERSON_GROUP_IF, "Mahasiswa IF ITB");

            new Program().AddPersonToGroup(PERSON_GROUP_IF, "Martino Christanto", @"D:\images\MartinoChristanto\");
            new Program().AddPersonToGroup(PERSON_GROUP_IF, "Steffi Indrayani", @"D:\images\SteffiIndrayani\");

            new Program().TrainingAI(PERSON_GROUP_IF);*/
            new Program().RecognitionFace(PERSON_GROUP_IF, @"D:\images\TesNinoTepi.jpg");
      
            Console.ReadLine();
        }
    }
}
