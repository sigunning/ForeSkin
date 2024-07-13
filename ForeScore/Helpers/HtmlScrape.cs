using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;


namespace ForeScore.Helpers

{
    public class HtmlScrape
    {
        
        public int[,] GetTable(string url)
        {

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(url);

            var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//table/tr[@class='nonfocus']");

            int[,] aData = new int[18,3];

            int iHole = 0;
            int iSet = 0;
            foreach (var node in htmlNodes)
            {

                // first 4 colums only
                for (int i = 0; i <= 3; i++)
                {
                    // only if contain a number
                    int number;
                    bool success = Int32.TryParse(node.ChildNodes[i].InnerHtml, out number);
                    if (success)
                    {
                        // store hole set to array
                        aData[iHole, iSet] = number;
                        
                        //output += number.ToString() + System.Environment.NewLine;

                        iSet++;
                        // new hole?
                        if (iSet >= 3)
                        {
                            iSet = 0;
                            iHole++;
                        }
                        
                    }
                    // stop after 18
                    if (iHole >= 18)
                        break;

                }

                
            }

            return aData;
        }

    }
}
