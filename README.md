Endpoints generales ofrecidas por la API

/members
    Registrar un nuevo miembro. Actualizar un miembro existente. Obtener un miembro especifico o una lista filtrada de miembros.
/genders
    Registro de un nuevo genero. Actualizar un genero existente. Obtener un genero especifico o una lista de generos existentes.
/plans
    Registro de un nuevo plan. Actualizar un plan existente. Obtener un plan especifico o una lista de planes existentes.
/subscriptions
    Registrar una nueva suscripcion a un miembro con un pago asociado de acuerdo al precio del plan y el cual puede incluir varios metodos de pago. Obtener suscripciones especificas o una lista filtrada. Borrado lógico para mantener un historial y evitar problemas de integridad en la base de datos.
/users
    Registrar un nuevo usuario. Actualizar un usuario existente. Obtener un usuario en especifico o una lista de todos los usuarios. Asignar roles a un usuario. Eliminar roles de un usuario. Deshabilitar a un usuario.
/auth
    Autenticar un usuario y generar tokens de acceso y un refresh token para la permanencia de la sesion. Cerrar sesion y revocar el refresh token. Obtener nuevos tokens de acceso mediante un refresh token.

Puedes encontrar el diagrama ER en el directorio raíz /docs
