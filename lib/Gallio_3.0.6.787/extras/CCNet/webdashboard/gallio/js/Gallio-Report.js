function toggle(id)
{
    var icon = document.getElementById('toggle-' + id);
    if (icon != null)
    {
        var childElement = document.getElementById(id);
        if (icon.src.indexOf('Plus.gif') != -1)
        {
            icon.src = icon.src.replace('Plus.gif', 'Minus.gif');
            if (childElement != null)
                childElement.style.display = "block";
        }
        else
        {
            icon.src = icon.src.replace('Minus.gif', 'Plus.gif');
            if (childElement != null)
                childElement.style.display = "none";
        }
    }
}

function expand(ids)
{
    for (var i = 0; i < ids.length; i++)
    {
        var id = ids[i];
        var icon = document.getElementById('toggle-' + id);
        if (icon != null)
        {
            if (icon.src.indexOf('Plus.gif') != -1)
            {
                icon.src = icon.src.replace('Plus.gif', 'Minus.gif');

                var childElement = document.getElementById(id);
                if (childElement != null)
                    childElement.style.display = "block";
            }
        }
    }
}

function navigateTo(path, line, column)
{
    var navigator = new ActiveXObject("Gallio.Navigator.GallioNavigator");
    if (navigator)
        navigator.NavigateTo(path, line, column);
}
