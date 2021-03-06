﻿using LC.Aplication.Book.DataTransferObject;
using LC.Commons;
using LC.Infrastruture.Repositories.Contracts;
using LC.Infrastruture.Repositories.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using LC.Application.Book.DataTransferObject;

namespace LC.Application.Book
{
    public class ApplicationBook
    {
        readonly IBookRepositoy _bookRepository;
        readonly GenerateSlug _generateSlug;

        public ApplicationBook(IBookRepositoy bookRepository , GenerateSlug generateSlug)
        {
            _bookRepository = bookRepository;
            _generateSlug = generateSlug;
        }

        public IEnumerable<Domain.Book> GetOrderedAscendingByName()
        {
            return _bookRepository.GetOrderedAscendingByName();
        }

        public Domain.Book GetById(int id)
        {
            return _bookRepository.Get(new Object[] { id });

        }

        public Domain.Book Created(CreatedBookDTO createdBookDTO)
        {
            try
            {
                _bookRepository.Begin();

                string slug = _generateSlug.generate(createdBookDTO.Name);

                if ( _bookRepository.CheckSlugCreated(slug) )
                {
                    throw new Exception("Livro já cadastrado com esse nome!");
                }



                Domain.Book _bk = new Domain.Book();

                _bk.Photo = this.uploadPhoto(createdBookDTO.Photo, slug);
                
                _bk.Author = createdBookDTO.Author;
                _bk.Slug = slug;
                _bk.DescriptionLong = createdBookDTO.DescriptionLong;
                _bk.DescriptionShort = createdBookDTO.DescriptionShort;
                _bk.Language = createdBookDTO.Language;
                _bk.Name = createdBookDTO.Name;
                _bk.Price = createdBookDTO.Price;
                _bk.Publishing = createdBookDTO.Publishing;
                _bk.QuantityPages = createdBookDTO.QuantityPages;
                _bk.Weight = createdBookDTO.Weight;
                _bk.Year = createdBookDTO.Year;
                _bk.CreatdAt = DateTime.Now;

                _bk = _bookRepository.Save(_bk);
                _bookRepository.Commit();
                return _bk;
            }
            catch (Exception ex)
            {
                _bookRepository.RollBack();
                throw ex;
            }
        }

        public Domain.Book Updated(int id, CreatedBookDTO createdBookDTO)
        {
            _bookRepository.Begin();

            try
            {

                Domain.Book _bk = _bookRepository.Get(new Object[] { id });
                               
                string slug = _generateSlug.generate(createdBookDTO.Name);

                if (createdBookDTO.Photo != null)
                {
                    _bk.Photo = uploadPhoto(createdBookDTO.Photo, slug);
                }

                _bk.Slug = slug;
                _bk.Author = createdBookDTO.Author;
                _bk.DescriptionLong = createdBookDTO.DescriptionLong;
                _bk.DescriptionShort = createdBookDTO.DescriptionShort;
                _bk.Language = createdBookDTO.Language;
                _bk.Name = createdBookDTO.Name;
                _bk.Price = createdBookDTO.Price;
                _bk.Publishing = createdBookDTO.Publishing;
                _bk.QuantityPages = createdBookDTO.QuantityPages;
                _bk.Weight = createdBookDTO.Weight;
                _bk.Year = createdBookDTO.Year;
                
                _bookRepository.Save(_bk);

                _bookRepository.Commit();
                return _bk;
            }
            catch (Exception ex)
            {
                _bookRepository.RollBack();
                throw ex;
            }
        }

        public void Delete(int id)
        {
            _bookRepository.Begin();

            try
            {
                Domain.Book _bk = _bookRepository.Get(new Object[] { id });

                if (_bk == null)
                {
                    throw new Exception("Livro informado não existe!");
                }

                //remover imagem do livro
                _bookRepository.Remove(_bk);
                _bookRepository.Commit();
            }
            catch (Exception ex)
            {
                _bookRepository.RollBack();
                throw ex;
            }
        }

        private string uploadPhoto(PhotoDTO photoDTO , string name)
        {
            
            var base64array = Convert.FromBase64String(photoDTO.Value);

            var directoryUpload = Directory.GetCurrentDirectory();
           
            var directoryServer = Path.Combine($"{Environment.GetEnvironmentVariable("DIRECTORY_IMAGE")}{name}.{photoDTO.FileType.Split('/')[1]}");
            
            var filePath = Path.Combine($"{directoryUpload}//wwwroot//{directoryServer}");
            System.IO.File.WriteAllBytes(filePath, base64array);

            return directoryServer;
        }
        
    }
}
