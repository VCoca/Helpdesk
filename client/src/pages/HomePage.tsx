import { Link } from 'react-router-dom'

function HomePage() {
    return (
        <div>
            <h1>Home</h1>
            <p>Welcome to Helpdesk</p>
            <Link to="/login">Log in</Link>
            {' | '}
            <Link to="/tickets">Tickets</Link>
        </div>
    )
}

export default HomePage
