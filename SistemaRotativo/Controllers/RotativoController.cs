using Newtonsoft.Json;
using SistemaRotativo.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Script.Serialization;
using SistemaRotativo;
using System.Data.Entity;

namespace SistemaRotativo.Controllers
{
    [RoutePrefix("api/Rotativo")]// onde eu tinha falado
    public class RotativoController : ApiController
    {
        private SistemaRotativoEntities sr;// esse ser aki faz a comunicação com o banco. Ele é gerado pelo entity
        
        public RotativoController()
        {
            this.sr = new SistemaRotativoEntities();//aki vc inicializa ele pra poder pegar os dadosss
        }

        [AcceptVerbs("GET")]// Esse krinha aki tá falando quue o método http que vai ser utilizado é o get
        [HttpGet]//esse aki tbm
        public DbSet<Rotativo> Listar()// Aki vc coloca o nome que vai chamar no postamn, ou no browser, com o retorno
        {
            return sr.Rotativo;//Aqui eu estou retornando todos os rotativos que eu tiver no banco
        }

        [AcceptVerbs("POST")]//Esse aqui faz o mesmo que o do GET, só que o método aqui é POST
        [Route("Comprar")]/// post pq não vai na url certo?Iiiiisso Estou inserindo dados no banco do caboclo lá ok 
        public Mensagem Comprar(Rotativo r)// Aqui é a msm coisa. Só q tem parametros.
        {
            //string placa = r.plaVei;
            //int qtdMinutos = r.qtdMin;
            bool flag = false;
            Mensagem mensagem = new Mensagem();
            foreach (Rotativo rotativo in (IEnumerable<Rotativo>)this.sr.Rotativo)
            {
                if (rotativo.plaVei.ToUpper() == r.plaVei.ToUpper() && rotativo.datSai > DateTime.Now && rotativo.datEnt.Date == DateTime.Now.Date)
                    flag = true;
            }
            if (flag)
            {
                mensagem.codigo = 1;
                mensagem.descricao = "A placa inserida já possui rotativo vinculado. Se desejar, renove-o!";
            }
            else
            {
                r.datEnt = DateTime.Now;
                r.valTotal = Convert.ToDecimal(0.1 * r.qtdMin);
                r.datSai = DateTime.Now.AddMinutes(r.qtdMin);
                this.sr.Rotativo.Add(r);
                this.sr.SaveChanges();
                mensagem.codigo = 2;
                mensagem.descricao = "Rotativo comprado com sucesso!";
            }
            return mensagem;
        }

        [AcceptVerbs("PUT")]//Esse tbm é o msm do GET, porém significa q vc está atualizando os dados, fazendo update
        [Route("Renovar")]//Quando a gnt coloca esse route aki, significa que vc está adicionando uma "camada"
        public Mensagem Renovar(Rotativo r)
        {
            bool flag = false;
            Mensagem mensagem = new Mensagem();
            Rotativo rotativo1 = new Rotativo();
            foreach (Rotativo rotativo2 in (IEnumerable<Rotativo>)this.sr.Rotativo)
            {
                if (rotativo2.plaVei.ToUpper() == r.plaVei.ToUpper())
                {
                    flag = true;
                    rotativo1 = rotativo2;
                }
            }
            if (flag)
            {
                if ((DateTime.Now - rotativo1.datEnt).TotalMinutes > (double)rotativo1.qtdMin)
                {
                    mensagem.codigo = 3;
                    mensagem.descricao = "Seu rotativo está vencido! Compre outro para continuar estacionado";
                }
                else
                {
                    rotativo1.qtdMin += r.qtdMin;
                    Rotativo rotativo2 = rotativo1;
                    rotativo2.valTotal = rotativo2.valTotal + Convert.ToDecimal(0.1 * (double)r.qtdMin);
                    this.sr.SaveChanges();
                    mensagem.codigo = 4;
                    mensagem.descricao = "Rotativo renovado com sucesso!";
                }
            }
            else
            {
                mensagem.codigo = 5;
                mensagem.descricao = "Esse veículo não está vinculado a nenhum rotativo!";
            }
            return mensagem;
        }


        [AcceptVerbs("GET")]
        [Route("Consultar/{placa}")]// Esse get aki é diferentão tá vendo! Ele já tem o nome do parametro, pra não precisar identificar em outro lugar
        public Mensagem Consultar(string placa)// blz
        {
            bool flag = false;
            Mensagem mensagem = new Mensagem();
            Rotativo rotativo1 = new Rotativo();
            foreach (Rotativo rotativo2 in (IEnumerable<Rotativo>)this.sr.Rotativo)
            {
                if (rotativo2.plaVei.ToUpper() == placa.ToUpper() && rotativo2.datEnt.Date == DateTime.Now.Date)
                {
                    flag = true;
                    rotativo1 = rotativo2;

                }
            }
            if (flag)
            {
                TimeSpan timeSpan = DateTime.Now - rotativo1.datEnt;
                if (timeSpan.TotalMinutes > (double)rotativo1.qtdMin)
                {
                    mensagem.codigo = 7;
                    mensagem.descricao = "Seu rotativo está vencido! Compre outro e evite ser multado.";
                }
                else
                {
                    double tempo = Math.Round((double)rotativo1.qtdMin - timeSpan.TotalMinutes, 2);
                    mensagem.codigo = 8;
                    mensagem.descricao = tempo.ToString();
                }
            }
            else
            {
                mensagem.codigo = 5;
                mensagem.descricao = "Esse veículo não está vinculado a nenhum rotativo!";
            }
            return mensagem;
        }
    }
}
