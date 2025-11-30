// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
document.addEventListener('DOMContentLoaded', function() {
  var brand = document.getElementById('nav-brand');
  if (!brand) return;
  brand.addEventListener('click', function() {
    brand.classList.add('loading');
  });
});
